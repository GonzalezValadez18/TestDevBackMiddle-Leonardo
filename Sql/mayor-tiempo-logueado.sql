USE CCenterRIA;
GO

-- Usuario con mayor tiempo logueado

SELECT TOP 1
    l.User_id,
    SUM(DATEDIFF(SECOND, l.fecha, o.fecha)) AS TotalSegundos,
    CONCAT(
        SUM(DATEDIFF(SECOND, l.fecha, o.fecha)) / 86400, ' días, ',
        (SUM(DATEDIFF(SECOND, l.fecha, o.fecha)) % 86400) / 3600, ' horas, ',
        (SUM(DATEDIFF(SECOND, l.fecha, o.fecha)) % 3600) / 60, ' minutos, ',
        SUM(DATEDIFF(SECOND, l.fecha, o.fecha)) % 60, ' segundos'
    ) AS TiempoTotal
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
GROUP BY l.User_id
ORDER BY TotalSegundos DESC;

GO