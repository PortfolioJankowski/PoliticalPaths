SELECT
    p.Imie,
    p.Nazwisko,
    YEAR(w.DataWyborow) AS RokWyborow,
    rw.Nazwa AS RodzajWyborow,
    ow.NumerOkregu,
    sw.NumerNaLiscie AS MiejsceNaLiscie,
    ww.LiczbaGlosow,
    ww.CzyMandat,
    kw.Nazwa AS Komitet
FROM StartyWyborcze sw

JOIN (
    SELECT PolitykId
    FROM StartyWyborcze
    GROUP BY PolitykId
    HAVING COUNT(*) > 1
) wiel
    ON wiel.PolitykId = sw.PolitykId

JOIN Politycy p
    ON p.Id = sw.PolitykId

JOIN WynikiWyborow ww
    ON ww.Id = sw.WynikiId

LEFT JOIN ListaWyborcza lw
    ON lw.Id = sw.ListaId

LEFT JOIN OkregWyborczy ow
    ON ow.Id = lw.OkregId

LEFT JOIN Wybory w
    ON w.Id = lw.WyboryId

LEFT JOIN RodzajeWyborow rw
    ON rw.Id = w.RodzajWyborowId

LEFT JOIN KomitetyWyborcze kw
    ON kw.Id = sw.KomitetId

ORDER BY
    p.Nazwisko,
    p.Imie,
    w.DataWyborow