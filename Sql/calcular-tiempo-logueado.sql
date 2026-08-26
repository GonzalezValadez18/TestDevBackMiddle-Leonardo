USE CCenterRIA;
GO

CREATE PROCEDURE CalcularTiempoLogueado
    @UserId INT,
    @FechaInicio DATETIME,
    @FechaFin DATETIME
AS
BEGIN

    SELECT
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
      AND l.User_id = @UserId
      AND l.fecha >= @FechaInicio
      AND o.fecha <= @FechaFin

    GROUP BY l.User_id;

END;
GO