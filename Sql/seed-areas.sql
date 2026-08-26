USE CCenterRIA;
GO

INSERT INTO ccRIACat_Areas
    (IDArea, AreaName, StatusArea, CreateDate)
VALUES
    (1, 'Default', 1, '2021-09-03T17:32:30.280'),
    (2, 'BBVA', 1, '2022-10-03T17:32:30'),
    (2, 'Banamex', 1, '2024-09-30T17:32:30');

GO

SELECT *
FROM ccRIACat_Areas;
GO