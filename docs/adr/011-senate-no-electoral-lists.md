# ADR-011: Senat bez list wyborczych

## Context

Walidacja względem Kodeksu wyborczy (dział IV, art. 264): wybory do Senatu odbywają się w 100 jednomandatowych okręgach, głos na jednego kandydata, bez list.

## Decision

`Candidacy.ElectoralListId` jest opcjonalne. Profil `SenateMajoritarian` wymaga `ElectoralDistrictId` + `ElectoralCommitteeId`, zakazuje `ElectoralListId`. Osobne transformery / reguły walidacji.

## Consequences

Model i fingerprint `Candidacy` zależą od `ElectionProfile`. Import list senackich jako `ElectoralList` jest błędem domenowym.
