using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedTunisiaHolidays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                MERGE [JourFerie] AS [target]
                USING
                (
                    VALUES
                        (CAST('2025-01-01' AS date), N'Jour de l''An'),
                        (CAST('2025-03-20' AS date), N'Fête de l''Indépendance'),
                        (CAST('2025-03-31' AS date), N'Aïd el-Fitr (1)'),
                        (CAST('2025-04-01' AS date), N'Aïd el-Fitr (2)'),
                        (CAST('2025-04-09' AS date), N'Journée des Martyrs'),
                        (CAST('2025-05-01' AS date), N'Fête du Travail'),
                        (CAST('2025-06-06' AS date), N'Aïd el-Idha (1)'),
                        (CAST('2025-06-07' AS date), N'Aïd el-Idha (2)'),
                        (CAST('2025-06-26' AS date), N'Jour de l''An hégirien'),
                        (CAST('2025-07-25' AS date), N'Fête de la République'),
                        (CAST('2025-08-13' AS date), N'Fête de la Femme'),
                        (CAST('2025-09-04' AS date), N'Mouled'),
                        (CAST('2025-10-15' AS date), N'Fête de l''Évacuation'),
                        (CAST('2025-12-17' AS date), N'Fête de la Révolution'),
                        (CAST('2026-01-01' AS date), N'Jour de l''An'),
                        (CAST('2026-03-20' AS date), N'Fête de l''Indépendance / Aïd el-Fitr (1)'),
                        (CAST('2026-03-21' AS date), N'Aïd el-Fitr (2)'),
                        (CAST('2026-04-09' AS date), N'Journée des Martyrs'),
                        (CAST('2026-05-01' AS date), N'Fête du Travail'),
                        (CAST('2026-05-26' AS date), N'Aïd el-Idha (1)'),
                        (CAST('2026-05-27' AS date), N'Aïd el-Idha (2)'),
                        (CAST('2026-06-16' AS date), N'Jour de l''An hégirien'),
                        (CAST('2026-07-25' AS date), N'Fête de la République'),
                        (CAST('2026-08-13' AS date), N'Fête de la Femme'),
                        (CAST('2026-08-25' AS date), N'Mouled'),
                        (CAST('2026-10-15' AS date), N'Fête de l''Évacuation'),
                        (CAST('2026-12-17' AS date), N'Fête de la Révolution'),
                        (CAST('2027-01-01' AS date), N'Jour de l''An'),
                        (CAST('2027-03-10' AS date), N'Aïd el-Fitr (1)'),
                        (CAST('2027-03-11' AS date), N'Aïd el-Fitr (2)'),
                        (CAST('2027-03-20' AS date), N'Fête de l''Indépendance'),
                        (CAST('2027-04-09' AS date), N'Journée des Martyrs'),
                        (CAST('2027-05-01' AS date), N'Fête du Travail'),
                        (CAST('2027-05-16' AS date), N'Aïd el-Idha (1)'),
                        (CAST('2027-05-17' AS date), N'Aïd el-Idha (2)'),
                        (CAST('2027-06-06' AS date), N'Jour de l''An hégirien'),
                        (CAST('2027-07-25' AS date), N'Fête de la République'),
                        (CAST('2027-08-13' AS date), N'Fête de la Femme'),
                        (CAST('2027-08-15' AS date), N'Mouled'),
                        (CAST('2027-10-15' AS date), N'Fête de l''Évacuation'),
                        (CAST('2027-12-17' AS date), N'Fête de la Révolution'),
                        (CAST('2028-01-01' AS date), N'Jour de l''An'),
                        (CAST('2028-02-27' AS date), N'Aïd el-Fitr (1)'),
                        (CAST('2028-02-28' AS date), N'Aïd el-Fitr (2)'),
                        (CAST('2028-03-20' AS date), N'Fête de l''Indépendance'),
                        (CAST('2028-04-09' AS date), N'Journée des Martyrs'),
                        (CAST('2028-05-01' AS date), N'Fête du Travail'),
                        (CAST('2028-05-05' AS date), N'Aïd el-Idha (1)'),
                        (CAST('2028-05-06' AS date), N'Aïd el-Idha (2)'),
                        (CAST('2028-05-25' AS date), N'Jour de l''An hégirien'),
                        (CAST('2028-07-25' AS date), N'Fête de la République'),
                        (CAST('2028-08-03' AS date), N'Mouled'),
                        (CAST('2028-08-13' AS date), N'Fête de la Femme'),
                        (CAST('2028-10-15' AS date), N'Fête de l''Évacuation'),
                        (CAST('2028-12-17' AS date), N'Fête de la Révolution'),
                        (CAST('2029-01-01' AS date), N'Jour de l''An'),
                        (CAST('2029-02-15' AS date), N'Aïd el-Fitr (1)'),
                        (CAST('2029-02-16' AS date), N'Aïd el-Fitr (2)'),
                        (CAST('2029-03-20' AS date), N'Fête de l''Indépendance'),
                        (CAST('2029-04-09' AS date), N'Journée des Martyrs'),
                        (CAST('2029-04-24' AS date), N'Aïd el-Idha (1)'),
                        (CAST('2029-04-25' AS date), N'Aïd el-Idha (2)'),
                        (CAST('2029-05-01' AS date), N'Fête du Travail'),
                        (CAST('2029-05-14' AS date), N'Jour de l''An hégirien'),
                        (CAST('2029-07-23' AS date), N'Mouled'),
                        (CAST('2029-07-25' AS date), N'Fête de la République'),
                        (CAST('2029-08-13' AS date), N'Fête de la Femme'),
                        (CAST('2029-10-15' AS date), N'Fête de l''Évacuation'),
                        (CAST('2029-12-17' AS date), N'Fête de la Révolution'),
                        (CAST('2030-01-01' AS date), N'Jour de l''An'),
                        (CAST('2030-02-04' AS date), N'Aïd el-Fitr (1)'),
                        (CAST('2030-02-05' AS date), N'Aïd el-Fitr (2)'),
                        (CAST('2030-03-20' AS date), N'Fête de l''Indépendance'),
                        (CAST('2030-04-09' AS date), N'Journée des Martyrs'),
                        (CAST('2030-04-13' AS date), N'Aïd el-Idha (1)'),
                        (CAST('2030-04-14' AS date), N'Aïd el-Idha (2)'),
                        (CAST('2030-05-01' AS date), N'Fête du Travail'),
                        (CAST('2030-05-03' AS date), N'Jour de l''An hégirien'),
                        (CAST('2030-07-12' AS date), N'Mouled'),
                        (CAST('2030-07-25' AS date), N'Fête de la République'),
                        (CAST('2030-08-13' AS date), N'Fête de la Femme'),
                        (CAST('2030-10-15' AS date), N'Fête de l''Évacuation'),
                        (CAST('2030-12-17' AS date), N'Fête de la Révolution'),
                        (CAST('2031-01-01' AS date), N'Jour de l''An'),
                        (CAST('2031-01-25' AS date), N'Aïd el-Fitr (1)'),
                        (CAST('2031-01-26' AS date), N'Aïd el-Fitr (2)'),
                        (CAST('2031-03-20' AS date), N'Fête de l''Indépendance'),
                        (CAST('2031-04-02' AS date), N'Aïd el-Idha (1)'),
                        (CAST('2031-04-03' AS date), N'Aïd el-Idha (2)'),
                        (CAST('2031-04-09' AS date), N'Journée des Martyrs'),
                        (CAST('2031-04-23' AS date), N'Jour de l''An hégirien'),
                        (CAST('2031-05-01' AS date), N'Fête du Travail'),
                        (CAST('2031-07-01' AS date), N'Mouled'),
                        (CAST('2031-07-25' AS date), N'Fête de la République'),
                        (CAST('2031-08-13' AS date), N'Fête de la Femme'),
                        (CAST('2031-10-15' AS date), N'Fête de l''Évacuation'),
                        (CAST('2031-12-17' AS date), N'Fête de la Révolution'),
                        (CAST('2032-01-01' AS date), N'Jour de l''An'),
                        (CAST('2032-01-14' AS date), N'Aïd el-Fitr (1)'),
                        (CAST('2032-01-15' AS date), N'Aïd el-Fitr (2)'),
                        (CAST('2032-03-20' AS date), N'Fête de l''Indépendance'),
                        (CAST('2032-03-22' AS date), N'Aïd el-Idha (1)'),
                        (CAST('2032-03-23' AS date), N'Aïd el-Idha (2)'),
                        (CAST('2032-04-09' AS date), N'Journée des Martyrs'),
                        (CAST('2032-04-11' AS date), N'Jour de l''An hégirien'),
                        (CAST('2032-05-01' AS date), N'Fête du Travail'),
                        (CAST('2032-06-19' AS date), N'Mouled'),
                        (CAST('2032-07-25' AS date), N'Fête de la République'),
                        (CAST('2032-08-13' AS date), N'Fête de la Femme'),
                        (CAST('2032-10-15' AS date), N'Fête de l''Évacuation'),
                        (CAST('2032-12-17' AS date), N'Fête de la Révolution')
                ) AS [source]([Date], [Label])
                ON [target].[Date] = [source].[Date]
                WHEN MATCHED THEN
                    UPDATE SET [Label] = [source].[Label]
                WHEN NOT MATCHED THEN
                    INSERT ([JourFerieId], [Date], [Label])
                    VALUES (NEWID(), [source].[Date], [source].[Label]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
