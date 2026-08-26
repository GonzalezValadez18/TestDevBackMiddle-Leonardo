USE CCenterRIA;
GO

-- Promedio de tiempo logueado por usuario en cada mes

SELECT
    l.User_id,
    YEAR(l.fecha) AS Anio,
    MONTH(l.fecha) AS Mes,
    AVG(DATEDIFF(SECOND, l.fecha, o.fecha)) AS PromedioSegundos,
    CONCAT(
        AVG(DATEDIFF(SECOND, l.fecha, o.fecha)) / 86400, ' días, ',
        (AVG(DATEDIFF(SECOND, l.fecha, o.fecha)) % 86400) / 3600, ' horas, ',
        (AVG(DATEDIFF(SECOND, l.fecha, o.fecha)) % 3600) / 60, ' minutos, ',
        AVG(DATEDIFF(SECOND, l.fecha, o.fecha)) % 60, ' segundos'
    ) AS PromedioTiempo
FROM ccloglogin l
INNER JOIN ccloglogin o
    ON l.User_id = o.User_id
    AND o.TipoMov = 0
    AND o.fecha = (
        SELECT MIN(fecha)
        FROM ccloglogin
        WHERE User_id = l.User_id
          AND TipoMov = 0
          AND fecha > l.fecha
    )
WHERE l.TipoMov = 1
GROUP BY
    l.User_id,
    YEAR(l.fecha),
    MONTH(l.fecha)
ORDER BY
    l.User_id,
    Anio,
    Mes;

GO