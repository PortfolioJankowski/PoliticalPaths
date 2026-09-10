# Logowanie, role i rate limiting

Dashboard używa ASP.NET Core Identity oraz cookie ważnego 8 godzin z przesuwanym terminem. Wszystkie Razor Pages wymagają logowania poza stroną logowania, odmowy dostępu i obsługi błędu.

- **User** przegląda Dashboard, importy, wybory, polityków i changelog.
- **Admin** ma dodatkowo panel użytkowników.
- Konto nieaktywne nie może się zalogować, a jego sesje są unieważniane przez security stamp.
- Konto utworzone lub zresetowane przez administratora musi zmienić hasło.
- Nie można zablokować ani zdegradować własnego konta lub ostatniego aktywnego administratora.

## Pierwszy administrator

Komendy sync i full-sync wymagają **Identity__SeedAdmin__Email** i **Identity__SeedAdmin__Password**. Ustawienie **Identity__SeedAdmin__Enabled=false** lub parametr **--no-identity-seed** jawnie pomija seed.

Seeder tworzy role i konto tylko wtedy, gdy ich brakuje. Nie resetuje istniejącego hasła i nie zapisuje go w logach.

## Hasła

Hasło ma co najmniej 12 znaków, małą i wielką literę, cyfrę oraz znak specjalny. Po pięciu błędach konto jest blokowane na 15 minut. Reset generuje losowe hasło tymczasowe pokazywane administratorowi tylko raz.

## Rate limiting

- Zalogowany: 120 żądań/minutę według NameIdentifier.
- Brak identyfikatora: 60 żądań/minutę według RemoteIpAddress.
- Logowanie: dodatkowo 20 żądań/5 minut po IP.
- Brak kolejki; przekroczenie zwraca 429 i Retry-After.

Limity są w sekcji RateLimiting i są lokalne dla procesu. Przy wielu replikach potrzebny będzie licznik rozproszony.

X-Forwarded-For jest honorowany wyłącznie dla bezpośredniego proxy z listy **RateLimiting:KnownProxies**. Produkcja wymaga HTTPS, ponieważ cookie ma flagę Secure. Sekrety należy podać z magazynu sekretów lub środowiska, nigdy z repozytorium.

Klucze Data Protection szyfrujące cookie są utrwalane w Dockerze w wolumenie **politicalpaths_dataprotection_keys**. Poza Dockerem ścieżkę ustawia **DataProtection:KeysPath**; utrata kluczy wyloguje wszystkie aktywne sesje.
