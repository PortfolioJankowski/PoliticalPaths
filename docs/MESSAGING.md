# Messaging, outbox i powiadomienia Discord

## Cel

Pierwszym asynchronicznym scenariuszem jest formularz kontaktowy. Dashboard zapisuje zgłoszenie oraz komunikat outbox w jednym `SaveChanges`, publisher dostarcza go do RabbitMQ, a `PoliticalPaths.NotificationWorker` przekazuje treść do webhooka Discord.

```text
Contact form -> ContactMessages + OutboxMessages -> RabbitMQ -> InboxMessages -> Discord webhook
```

RabbitMQ nie jest źródłem prawdy. Zgłoszenie pozostaje w MariaDB nawet wtedy, gdy broker albo Discord są czasowo niedostępne.

## Konfiguracja

Topologia znajduje się w `config/rabbitmq.topology.json`. Można dodawać tam exchanges, queues i bindings. Podczas uruchamiania Dashboard i worker deklarują infrastrukturę idempotentnie. Provisioner nie usuwa istniejących elementów.

Sekrety pochodzą wyłącznie ze zmiennych środowiskowych:

- `RABBITMQ_USERNAME`, `RABBITMQ_PASSWORD`, `RABBITMQ_VHOST`;
- `RABBITMQ_HOST`, `RABBITMQ_PORT`, `RABBITMQ_USE_TLS`;
- `DISCORD_WEBHOOK_URL` i opcjonalnie `DISCORD_USERNAME`.
- `SENDGRID_ENABLED`, `SENDGRID_API_KEY`, `SENDGRID_FROM_EMAIL` oraz opcjonalnie `SENDGRID_FROM_NAME`. Gdy integracja jest wyłączona, worker Discord nadal działa, a e-maile pozostają w outboxie.

Przykład znajduje się w `.env-example`. Prawdziwy `.env` jest ignorowany przez Git.

## Gwarancje dostarczenia

Publisher używa trwałych wiadomości i publisher confirms. Nieprzetworzone rekordy outbox są ponawiane z rosnącym opóźnieniem. Konsument działa w modelu at-least-once i zapisuje identyfikator komunikatu w `InboxMessages`, aby nie obsługiwać ponownie potwierdzonej dostawy. Kolejka Discorda jest kolejką quorum; po pięciu nieudanych dostawach wiadomość przechodzi do `notifications.discord.dead-letter`.

Webhook Discord jest systemem zewnętrznym, dlatego awaria procesu dokładnie pomiędzy przyjęciem odpowiedzi Discorda a zapisem inbox może wyjątkowo spowodować duplikat. Identyfikator `ContactMessage` w stopce pozwala go rozpoznać.

## Bezpieczeństwo formularza

- dostęp wyłącznie po zalogowaniu;
- limit 3 wiadomości na godzinę per użytkownik, z fallbackiem po IP;
- honeypot dla prostych botów;
- limity długości pól po stronie formularza i bazy;
- Discord `allowed_mentions` wyłącza `@everyone`, role i automatyczne wzmianki;
- webhook oraz hasło brokera nie są logowane.

## Alerty e-mail

Użytkownik sam włącza zgodę na stronie `/Account/Settings`. Administrator tworzy kampanię na `/Admin/Email`; odbiorcami są wyłącznie aktywne konta z potwierdzonym adresem i aktywną zgodą. Kampania, dostarczenia i rekordy outbox powstają atomowo. Worker pobiera `notification.email.#`, wysyła wiadomości przez REST API SendGrid i zapisuje status oraz identyfikator dostawcy.
