# Walidacja modelu domenowego — Kodeks wyborczy i praktyka

Dokument weryfikuje [05-domain-model.md](05-domain-model.md) względem ustawy z 5.01.2011 r. **Kodeks wyborczy** (Dz.U. 2022 poz. 1277 z późn. zm.) oraz praktyki PKW/Sejmu. To nie jest porada prawna — przy implementacji odwołuj się do aktualnego tekstu jednolitego i załączników.

**Data walidacji:** 2026-05-20

---

## Werdykt ogólny

| Obszar modelu | Ocena | Uwagi |
|---------------|-------|-------|
| Okręg per `Election` + `ElectoralChamber` | ✅ Zgodne | Art. 201–203 (Sejm), dział IV (Senat), art. 462 (sejmiki) |
| TERYT M:N z okręgiem | ✅ Zasadne | Granice okręgów wiążą powiaty/gminy; TERYT do map, nie do „startu” |
| Snapshot statystyk okręgu | ✅ Zgodne | Liczba mieszkańców wpływa na mandaty (art. 202); PKW publikuje też uprawnionych — inne w każdych wyborach |
| Lista w okręgu (Sejm / sejmik) | ✅ Zgodne | Art. 209–213, jedna lista komitetu na okręg |
| `Candidacy` z obowiązkową listą | ⚠️ **Wymaga korekty** | **Senat i wybory prezydenckie nie używają list w tym samym sensie** |
| `Party` na liście | ⚠️ Uściślić | Z listą wiąże się **komitet wyborczy** (art. 96–99, 209), nie zawsze jedna partia |
| `ParliamentaryClub` | ✅ Zasadne | Regulamin Sejmu (art. 8), nie Kodeks wyborczy — osobna faza życia polityka |
| Wyniki per rok / wybory | ✅ Zgodne | Wyniki są zawsze dla konkretnych wyborów |
| Brak obwodu wyborczego | ⚠️ Świadomy zakres | PKW publikuje też obwody — model na start może być na poziomie okręgu |

---

## 1. Sejm RP (dział III, art. 193–262)

### Zgodne z modelem

- **41 okręgów wielomandatowych**, 460 posłów — załącznik nr 1 do KW (numery, granice, liczba posłów w okręgu).
- **Granice okręgu** nie mogą naruszać granic powiatów i miast na prawach powiatu (art. 201 § 3).
- **Lista kandydatów** zgłaszana przez komitet wyborczy **w danym okręgu**; w jednym okręgu komitet może zgłosić **tylko jedną** listę (art. 209).
- Kandydat na posła: **jeden okręg, jedna lista** (nie w dwóch okręgach / listach).
- **Nie można** kandydować równocześnie na posła i senatora (ogólna zasada kwalifikacji kandydatów).
- **Liczba mandatów w okręgu** zmienia się z przeliczeniem normy przedstawicielstwa (art. 202) — uzasadnia `ElectoralDistrictSnapshot` + pole **`SeatsAllocated`** (liczba posłów wybieranych w okręgu w tych wyborach).
- Wyborca głosuje na **jedną listę**, wskazując pierwszeństwo kandydata na liście (art. 227) — uzasadnia osobno `ElectoralListVoteResult` i `CandidacyVoteResult` (głosy na kandydatów / preferencyjne).

### Uściślenia do modelu

| Element | Rekomendacja |
|---------|----------------|
| Podmiot listy | Encja **`ElectoralCommittee`** (komitet wyborczy / koalicyjny); `Party` opcjonalnie (wiele komitetów to koalicje) |
| `ElectoralList` | FK do `ElectoralCommitteeId` + opcjonalnie `PartyId` |
| Mandaty w okręgu | `ElectoralDistrictSnapshot.SeatsAllocated` z załącznika do tych wyborów |
| Mieszkańcy vs uprawnieni | Oba w snapshot; art. 202 operuje na **ludności**, PKW w protokołach — na **wyborcach uprawnionych** |

---

## 2. Senat RP (dział IV, art. 261–290)

### Kluczowa różnica prawna

Wybory do Senatu od 2011 r.: **100 jednomandatowych okręgów**, wybór **bez list** — wyborca oddaje głos na **jednego kandydata**, zwycięzca = najwięcej głosów w okręgu.

- Komitet może zgłosić w okręgu **tylko jednego** kandydata na senatora (art. 264).
- Kandydat w **jednym** okręgu, jeden komitet.
- Okręgi senackie **nie mogą naruszać granic okręgów sejmowych** (art. 204) — inna siatka, inna numeracja (np. w Małopolsce okręgi sejmowe 12–15 i senackie 30–37).

### Korekta modelu (konieczna)

```
Candidacy.ElectoralListId  →  NULLABLE
Candidacy.ElectoralCommitteeId  →  wymagane dla Senatu (kto zgłosił kandydata)
```

Dla Senatu **nie tworzymy** `ElectoralList` — transformer senacki omija listy.

`SourceFingerprint` dla Senatu: hash(`ElectionId`, `PoliticianId`, `DistrictId`, `CommitteeId`) — bez `ListId`.

Wyniki: `CandidacyVoteResult` (głosy bezpośrednie, `Elected` = mandat senacki).

---

## 3. Sejmiki województw (rozdz. 12 KW, m.in. art. 462)

### Zgodne

- Podział obszaru **województwa** na okręgi (często powiat lub część powiatu; łączenie powiatów przy małej liczbie radnych — art. 462).
- **Listy kandydatów w okręgach** — analogicznie do Sejmu (proporcjonalny podział mandatów w obrębie okręgu).
- **`Election`** powinien mieć `VoivodeshipTerritorialUnitId` — wybory sejmikowe są **w skali województwa**, numeracja okręgów resetuje się w każdym województwie (np. okręg 1 w pomorskim ≠ okręg 1 w mazowieckim).

### Uściślenia

| Element | Rekomendacja |
|---------|----------------|
| `ElectoralChamber.RegionalAssembly` | OK; `NaturalKey` wyborów: `sejmik-{województwo}-{rok}` |
| `ElectoralDistrict.DistrictNumber` | Unikalny w ramach `(ElectionId)` — nie globalnie w kraju |
| Granice vs TERYT | Często 1 okręg ≈ 1 powiat (TERYT powiat) — M:N nadal poprawne przy podziale powiatu |

---

## 4. TERYT a okręg wyborczy

### Zgodne z intencją modelu

- Okręg to konstrukcja **prawno-wyborcza**, nie jeden kod TERYT.
- Mapowanie **powiat / gmina / województwo** do okręgu jest poprawne jako `ElectoralDistrictTerritory`.
- **Miejsce zamieszkania wyborcy** decyduje, w którym okręgu głosuje (art. 17, 18 KW) — to reguła dla przyszłej analizy geograficznej, nie FK na `Candidacy`.

### Uwaga implementacyjna

- **Obwód wyborczy** (głosowanie w szkole) ≠ **okręg wyborczy**. Dane PKW bywają na poziomie obwodu, gminy i okręgu — w dokumentacji przyjmij **domyślny poziom agregacji: okręg**; obwody jako osobna encja tylko jeśli importujesz protokoły szczegółowe.

---

## 5. Statystyki okręgu (mieszkańcy, uprawnieni)

| Źródło prawne | Pole w modelu |
|---------------|---------------|
| Art. 202 — norma przedstawicielstwa na podstawie **ludności** | `Population` |
| Protokoły PKW — wyborcy **uprawnieni** | `EligibleVoters` |
| Frekwencja / głosy ważne | `DistrictTurnoutResult` lub rozszerzenie snapshot / osobna tabela wyników okręgu |

Wersjonowanie per `Election` — **poprawne**: przed wyborami 2019 i 2023 inna liczba uprawnionych w tym samym numerze okręgu sejmowego jest normalna (zmiana granic, ludności, spisów).

Dodaj do snapshot: **`SeatsAllocated`** (mandaty w okręgu według załącznika KW dla tych wyborów).

---

## 6. Partia, komitet wyborczy, klub parlamentarny

| Pojęcie | Podstawa | W modelu |
|---------|----------|----------|
| **Komitet wyborczy** | KW — zgłaszanie list/kandydatów | **`ElectoralCommittee`** (brakowało) |
| **Partia polityczna** | Rejestr MS / współtwórca koalicji | `Party` — OK |
| **Koalicyjny komitet wyborczy** | Art. 87, 99 | `ElectoralCommittee` typu `Coalition` + skład |
| **Klub poselski** | Regulamin Sejmu art. 8 (≥15 posłów) | `ParliamentaryClub` + `ClubMembership` — OK, **po wyborach** |
| **Koło poselskie** | Regulamin art. 8 (<15) | Opcjonalnie `ParliamentaryCircle` — niski priorytet |

**Praktyka:** poseł może startować z komitetu „Trzecia Droga”, a po wyborach siedzieć w klubie innej nazwy — dlatego **`PartyAffiliation`** i **`ClubMembership`** z datami są konieczne i **nie** zastępują ich `ElectoralList.PartyId`.

---

## 7. Wybory nieobjęte obecnym modelem (świadome rozszerzenia)

| Typ wyborów | Okręgi | Listy | Uwaga |
|-------------|--------|-------|-------|
| **Prezydent RP** | Brak okręgów kandydackich (kraj) | Lista kandydatów **państwowa** (PKW), nie per okręg | Osobny profil: `Candidacy` bez `District`/`List` lub `PresidentialElection` |
| **Europarlament** | Okręgi krajowe (7 od 2004) | Listy w okręgu | `ElectoralChamber.EuropeanParliament` — później |
| **Wybory do gmin/powiatów** | Inna logika (art. 400+) | Często imienne | Osobny `ElectionScope` — poza pierwszą iteracją |

---

## 8. Reguły biznesowe (walidacja w transformatorze)

| Reguła | Podstawa |
|--------|----------|
| Ten sam polityk nie może mieć `Candidacy` na Sejm i Senat w tych samych wyborach parlamentarnych | KW — zakaz łącznego kandydowania |
| Sejm: `ElectoralListId` wymagane | Art. 209 |
| Senat: `ElectoralListId` musi być NULL | Dział IV |
| Lista musi należeć do tego samego `ElectoralDistrictId` co `Candidacy` | Art. 209 |
| `Chamber` na `District` musi zgadzać się z `Election.Chamber` | Spójność modelu |

---

## 9. Podsumowanie zmian w [05-domain-model.md](05-domain-model.md)

Wprowadzone po walidacji (sekcja „Korekty po walidacji KW”):

1. `ElectoralCommittee` + FK z `ElectoralList` i `Candidacy`.
2. `Candidacy.ElectoralListId` — **opcjonalne** (wymagane tylko dla profilu listowego: Sejm, sejmik).
3. `ElectoralDistrictSnapshot.SeatsAllocated`.
4. Rozróżnienie **okręg vs obwód**.
5. `Election.VoivodeshipTerritorialUnitId` dla sejmików.
6. Profil wyborów (`ElectionProfile` / typ wyborów) zamiast zakładania jednego kształtu `Candidacy`.

---

## 10. Mandat w kadencji (uzupełnienie po walidacji)

Model [10-mandate-lifecycle.md](10-mandate-lifecycle.md) jest **zgodny z KW**:

| Mechanizm | Podstawa |
|-----------|----------|
| Wygaśnięcie mandatu posła | Art. 247–249 (przesłanki, postanowienie Marszałka) |
| Obsadzenie z listy w trakcie kadencji | Art. 251, 233 (kolejność głosów, 7 dni na oświadczenie) |
| Wybory uzupełniające do Senatu | Art. 283 |
| Wygaśnięcie mandatu radnego (sejmik) | Art. 383 (+ przepisy uzupełniające rozdz. 12) |

**Rozdzielenie `Elected` vs `Mandate`** — wymagane prawem i praktyką: alokacja mandatu po wyborach nie oznacza jeszcze pełnienia (ślubowanie, odmowa); mandat może wygasnąć bez nowych wyborów powszechnych (następca z listy).

---

## Źródła (skrót)

- [Kodeks wyborczy — ISAP / ELI](https://api.sejm.gov.pl/eli/acts/DU/2022/1277/text.html)
- [PKW — zasady zgłaszania list i kandydatów](https://pkw.gov.pl/)
- [Regulamin Sejmu — kluby poselskie (art. 8)](https://www.sejm.gov.pl/prawo/regulamin/regsejm.htm)
- Art. 201–203, 209, 227, 202 — Sejm; art. 204, 264 — Senat; art. 462 — sejmiki
