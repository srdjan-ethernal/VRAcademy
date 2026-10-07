using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VRAcademy.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogCourses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "Code", "DescriptionEn", "DescriptionSr", "NameEn", "NameSr", "PassScore", "ValidityMonths" },
                values: new object[,]
                {
                    { new Guid("12996b73-45c7-456f-bdbb-617c47786e1b"), "lockout-tagout", "Energy source identification, isolation, lockout, and safe equipment verification.", "Identifikacija izvora energije, izolacija, zakljucavanje i bezbedna provera opreme.", "Lockout / Tagout", "Lockout/Tagout (LOTO)", 80, 12 },
                    { new Guid("16b3545f-ed63-426a-88e3-64a016c44540"), "electrical-safety", "Electrical hazard recognition, safe work practices, and emergency response.", "Prepoznavanje elektricnih rizika, bezbedan rad i reagovanje u vanrednim situacijama.", "Electrical Safety", "Elektricna bezbednost", 80, 12 },
                    { new Guid("3259a124-f4d3-4159-a195-7bed362c5b06"), "industrial-machinery-safety", "Machine inspection, guard checks, safe startup, operation, and shutdown.", "Pregled masine, provera zastita, bezbedno pokretanje i zaustavljanje opreme.", "Safe Operation of Industrial Machinery", "Bezbedno rukovanje industrijskim masinama", 80, 12 },
                    { new Guid("403f6449-7b47-4505-9bc3-d7f057b8b562"), "laboratory-safety", "Safe work with samples, laboratory equipment, chemicals, and contamination.", "Bezbedan rad sa uzorcima, laboratorijskom opremom, hemikalijama i kontaminacijom.", "Laboratory Safety", "Bezbednost u laboratoriji", 80, 12 },
                    { new Guid("448882ac-404c-4385-a47e-fa6f47c362a6"), "drone-operation-fpv", "Pre-flight checks, safe operation, FPV navigation, and incident response.", "Predletna provera, bezbedno upravljanje, FPV navigacija i reagovanje na incidente.", "Drone Operation and FPV Navigation", "Upravljanje dronom i FPV navigacija", 80, 12 },
                    { new Guid("5cadaf82-f481-4d73-9fdf-46bcb6ab5039"), "hazardous-material-handling", "Receiving, labeling, segregation, transport, and storage of hazardous materials.", "Prijem, oznacavanje, razdvajanje, transport i skladistenje opasnih materijala.", "Hazardous Material Handling", "Rukovanje opasnim materijalima", 80, 12 },
                    { new Guid("6c9def13-100e-492f-a464-773343a8d8b6"), "gas-leak-response", "Leak recognition, alarm activation, area isolation, and safe evacuation.", "Prepoznavanje curenja, aktiviranje alarma, izolacija zone i bezbedna evakuacija.", "Gas Leak Response", "Reagovanje pri curenju gasa", 80, 12 },
                    { new Guid("7a1791c9-805d-48c0-a8a5-977d2563e602"), "drone-search-rescue", "Mission planning, aerial search, casualty location, and rescue coordination.", "Planiranje misije, vazdusna pretraga, lociranje osobe i koordinacija spasavanja.", "Drone-Assisted Search and Rescue", "Potraga i spasavanje uz pomoc drona", 80, 12 },
                    { new Guid("8c8ce09e-bc0c-4254-be44-c382607d1a34"), "warehouse-logistics-safety", "Safe movement, load stacking, equipment handling, and logistics risk control.", "Bezbedno kretanje, slaganje tereta, rukovanje opremom i kontrola logistickih rizika.", "Warehouse and Logistics Safety", "Bezbednost u skladistu i logistici", 80, 12 },
                    { new Guid("9145acd3-4b9b-44fb-bdfb-131bc25ed151"), "working-at-heights", "Fall hazard recognition, equipment selection, and safe movement at height.", "Prepoznavanje rizika od pada, izbor opreme i bezbedno kretanje na visini.", "Working at Heights", "Rad na visini", 80, 12 },
                    { new Guid("92bab77b-a886-4e43-b62c-746c047b4810"), "personal-protective-equipment", "Risk assessment, selection, inspection, correct use, and maintenance of PPE.", "Procena rizika, izbor, pregled, pravilno koriscenje i odrzavanje zastitne opreme.", "Personal Protective Equipment", "Licna zastitna oprema", 80, 12 },
                    { new Guid("9b599484-289e-4273-914a-95e2bf142bbf"), "construction-site-safety", "Hazard recognition, safe movement, work zones, scaffolding, and access control.", "Prepoznavanje rizika, bezbedno kretanje, radne zone, skele i kontrola pristupa.", "Construction Site Safety", "Bezbednost na gradilistu", 80, 12 },
                    { new Guid("a1859120-8276-4ec7-934a-0d1e0a102507"), "emergency-evacuation", "Alarm recognition, exit selection, assisting others, and safe assembly.", "Prepoznavanje alarma, izbor izlaza, pomoc kolegama i bezbedno okupljanje.", "Emergency Evacuation", "Hitna evakuacija", 80, 12 },
                    { new Guid("a6d8459a-ccc9-436e-a670-72f6f925f7ed"), "confined-space-entry", "Space assessment, atmospheric testing, work permits, and rescue procedures.", "Procena prostora, merenje atmosfere, dozvola za rad i postupci spasavanja.", "Confined Space Entry", "Ulazak u zatvoreni prostor", 80, 12 },
                    { new Guid("b7e813a7-c8b3-4348-a6f3-32b21df00bb9"), "crane-suspended-loads", "Lift planning, exclusion zones, communication, and response to unstable loads.", "Planiranje dizanja, bezbedne zone, komunikacija i reagovanje pri nestabilnom teretu.", "Working Near Cranes and Suspended Loads", "Rad u blizini dizalica i visecih tereta", 80, 12 },
                    { new Guid("bb8111a3-b58d-403a-9245-63961f2f291b"), "first-aid", "Situation assessment, CPR, bleeding control, and correct use of an AED.", "Procena situacije, KPR, kontrola krvarenja i pravilna upotreba AED uredjaja.", "First Aid Training", "Prva pomoc", 80, 12 },
                    { new Guid("e31f5b2a-7cee-4731-a8f1-b5cc2f455a3e"), "forklift-safety", "Vehicle inspection, safe operation, load handling, and workplace navigation.", "Pregled vozila, bezbedno upravljanje, rukovanje teretom i kretanje kroz radnu zonu.", "Forklift Safety", "Bezbedno upravljanje viljuskarom", 80, 12 },
                    { new Guid("e3d1e155-8245-40ce-8820-158939f94d08"), "chemical-safety", "Label recognition, safe chemical handling, and spill response.", "Prepoznavanje oznaka, bezbedno rukovanje hemikalijama i reagovanje pri prosipanju.", "Chemical Safety", "Hemijska bezbednost", 80, 12 }
                });

            migrationBuilder.InsertData(
                table: "TrainingScenarios",
                columns: new[] { "Id", "Code", "DescriptionEn", "DescriptionSr", "EstimatedMinutes", "NameEn", "NameSr", "RiskCategory" },
                values: new object[,]
                {
                    { new Guid("00cf06fa-8954-46c3-be43-2276b24864b4"), "industrial-machinery-safety", "Machine inspection, guard checks, safe startup, operation, and shutdown.", "Pregled masine, provera zastita, bezbedno pokretanje i zaustavljanje opreme.", 30, "Safe Operation of Industrial Machinery", "Bezbedno rukovanje industrijskim masinama", "Machinery safety" },
                    { new Guid("2ecb069d-56eb-4b12-848e-6fb847f0352a"), "gas-leak-response", "Leak recognition, alarm activation, area isolation, and safe evacuation.", "Prepoznavanje curenja, aktiviranje alarma, izolacija zone i bezbedna evakuacija.", 30, "Gas Leak Response", "Reagovanje pri curenju gasa", "Emergency response" },
                    { new Guid("2f0339c9-8371-45d2-a844-2d208468dad4"), "emergency-evacuation", "Alarm recognition, exit selection, assisting others, and safe assembly.", "Prepoznavanje alarma, izbor izlaza, pomoc kolegama i bezbedno okupljanje.", 25, "Emergency Evacuation", "Hitna evakuacija", "Emergency response" },
                    { new Guid("303c1d09-c3ac-4953-a47b-58329bea4bcc"), "drone-search-rescue", "Mission planning, aerial search, casualty location, and rescue coordination.", "Planiranje misije, vazdusna pretraga, lociranje osobe i koordinacija spasavanja.", 35, "Drone-Assisted Search and Rescue", "Potraga i spasavanje uz pomoc drona", "Search and rescue" },
                    { new Guid("3ba2f8b1-126d-4251-ae77-ea977be1304b"), "construction-site-safety", "Hazard recognition, safe movement, work zones, scaffolding, and access control.", "Prepoznavanje rizika, bezbedno kretanje, radne zone, skele i kontrola pristupa.", 30, "Construction Site Safety", "Bezbednost na gradilistu", "Construction safety" },
                    { new Guid("493d682f-7cbd-445a-a18f-2a2d05f55434"), "crane-suspended-loads", "Lift planning, exclusion zones, communication, and response to unstable loads.", "Planiranje dizanja, bezbedne zone, komunikacija i reagovanje pri nestabilnom teretu.", 30, "Working Near Cranes and Suspended Loads", "Rad u blizini dizalica i visecih tereta", "Lifting operations" },
                    { new Guid("5666ad20-8b6e-497f-aa65-5d74003b9fc3"), "personal-protective-equipment", "Risk assessment, selection, inspection, correct use, and maintenance of PPE.", "Procena rizika, izbor, pregled, pravilno koriscenje i odrzavanje zastitne opreme.", 20, "Personal Protective Equipment", "Licna zastitna oprema", "Personal protection" },
                    { new Guid("5bd2218a-d12f-4489-a6f4-de90321c3289"), "laboratory-safety", "Safe work with samples, laboratory equipment, chemicals, and contamination.", "Bezbedan rad sa uzorcima, laboratorijskom opremom, hemikalijama i kontaminacijom.", 30, "Laboratory Safety", "Bezbednost u laboratoriji", "Laboratory safety" },
                    { new Guid("5eec99b0-5277-4d75-b2c4-8db0587495d1"), "chemical-safety", "Label recognition, safe chemical handling, and spill response.", "Prepoznavanje oznaka, bezbedno rukovanje hemikalijama i reagovanje pri prosipanju.", 30, "Chemical Safety", "Hemijska bezbednost", "Chemical safety" },
                    { new Guid("60a98bf7-169c-42ef-ba03-fad6e6494c30"), "confined-space-entry", "Space assessment, atmospheric testing, work permits, and rescue procedures.", "Procena prostora, merenje atmosfere, dozvola za rad i postupci spasavanja.", 30, "Confined Space Entry", "Ulazak u zatvoreni prostor", "Confined spaces" },
                    { new Guid("6140c520-3b69-4019-923e-c2b402a05a96"), "working-at-heights", "Fall hazard recognition, equipment selection, and safe movement at height.", "Prepoznavanje rizika od pada, izbor opreme i bezbedno kretanje na visini.", 30, "Working at Heights", "Rad na visini", "Work at height" },
                    { new Guid("80286c22-aa3f-457c-a308-f01542307c02"), "forklift-safety", "Vehicle inspection, safe operation, load handling, and workplace navigation.", "Pregled vozila, bezbedno upravljanje, rukovanje teretom i kretanje kroz radnu zonu.", 30, "Forklift Safety", "Bezbedno upravljanje viljuskarom", "Industrial vehicles" },
                    { new Guid("a6bfca41-c56d-4d4d-a38c-016e94d8b841"), "warehouse-logistics-safety", "Safe movement, load stacking, equipment handling, and logistics risk control.", "Bezbedno kretanje, slaganje tereta, rukovanje opremom i kontrola logistickih rizika.", 25, "Warehouse and Logistics Safety", "Bezbednost u skladistu i logistici", "Logistics safety" },
                    { new Guid("a6f0a23f-0d19-4af4-b4a2-75525d82c53b"), "hazardous-material-handling", "Receiving, labeling, segregation, transport, and storage of hazardous materials.", "Prijem, oznacavanje, razdvajanje, transport i skladistenje opasnih materijala.", 30, "Hazardous Material Handling", "Rukovanje opasnim materijalima", "Hazardous materials" },
                    { new Guid("ccb1504c-7ecb-4558-8634-5dacfdb1aec4"), "first-aid", "Situation assessment, CPR, bleeding control, and correct use of an AED.", "Procena situacije, KPR, kontrola krvarenja i pravilna upotreba AED uredjaja.", 30, "First Aid Training", "Prva pomoc", "Emergency response" },
                    { new Guid("de341505-37ae-4dde-b647-7a2b7ae0094d"), "drone-operation-fpv", "Pre-flight checks, safe operation, FPV navigation, and incident response.", "Predletna provera, bezbedno upravljanje, FPV navigacija i reagovanje na incidente.", 30, "Drone Operation and FPV Navigation", "Upravljanje dronom i FPV navigacija", "Drone operations" },
                    { new Guid("e357ce88-65e5-413c-acb3-6b6a8ac35b08"), "electrical-safety", "Electrical hazard recognition, safe work practices, and emergency response.", "Prepoznavanje elektricnih rizika, bezbedan rad i reagovanje u vanrednim situacijama.", 30, "Electrical Safety", "Elektricna bezbednost", "Electrical safety" },
                    { new Guid("f21e5460-36b5-4fe1-b3c7-2d1b9f0c6694"), "lockout-tagout", "Energy source identification, isolation, lockout, and safe equipment verification.", "Identifikacija izvora energije, izolacija, zakljucavanje i bezbedna provera opreme.", 25, "Lockout / Tagout", "Lockout/Tagout (LOTO)", "Energy control" }
                });

            migrationBuilder.InsertData(
                table: "CourseScenarios",
                columns: new[] { "CourseId", "ScenarioId" },
                values: new object[,]
                {
                    { new Guid("12996b73-45c7-456f-bdbb-617c47786e1b"), new Guid("f21e5460-36b5-4fe1-b3c7-2d1b9f0c6694") },
                    { new Guid("16b3545f-ed63-426a-88e3-64a016c44540"), new Guid("e357ce88-65e5-413c-acb3-6b6a8ac35b08") },
                    { new Guid("3259a124-f4d3-4159-a195-7bed362c5b06"), new Guid("00cf06fa-8954-46c3-be43-2276b24864b4") },
                    { new Guid("403f6449-7b47-4505-9bc3-d7f057b8b562"), new Guid("5bd2218a-d12f-4489-a6f4-de90321c3289") },
                    { new Guid("448882ac-404c-4385-a47e-fa6f47c362a6"), new Guid("de341505-37ae-4dde-b647-7a2b7ae0094d") },
                    { new Guid("5cadaf82-f481-4d73-9fdf-46bcb6ab5039"), new Guid("a6f0a23f-0d19-4af4-b4a2-75525d82c53b") },
                    { new Guid("6c9def13-100e-492f-a464-773343a8d8b6"), new Guid("2ecb069d-56eb-4b12-848e-6fb847f0352a") },
                    { new Guid("7a1791c9-805d-48c0-a8a5-977d2563e602"), new Guid("303c1d09-c3ac-4953-a47b-58329bea4bcc") },
                    { new Guid("8c8ce09e-bc0c-4254-be44-c382607d1a34"), new Guid("a6bfca41-c56d-4d4d-a38c-016e94d8b841") },
                    { new Guid("9145acd3-4b9b-44fb-bdfb-131bc25ed151"), new Guid("6140c520-3b69-4019-923e-c2b402a05a96") },
                    { new Guid("92bab77b-a886-4e43-b62c-746c047b4810"), new Guid("5666ad20-8b6e-497f-aa65-5d74003b9fc3") },
                    { new Guid("9b599484-289e-4273-914a-95e2bf142bbf"), new Guid("3ba2f8b1-126d-4251-ae77-ea977be1304b") },
                    { new Guid("a1859120-8276-4ec7-934a-0d1e0a102507"), new Guid("2f0339c9-8371-45d2-a844-2d208468dad4") },
                    { new Guid("a6d8459a-ccc9-436e-a670-72f6f925f7ed"), new Guid("60a98bf7-169c-42ef-ba03-fad6e6494c30") },
                    { new Guid("b7e813a7-c8b3-4348-a6f3-32b21df00bb9"), new Guid("493d682f-7cbd-445a-a18f-2a2d05f55434") },
                    { new Guid("bb8111a3-b58d-403a-9245-63961f2f291b"), new Guid("ccb1504c-7ecb-4558-8634-5dacfdb1aec4") },
                    { new Guid("e31f5b2a-7cee-4731-a8f1-b5cc2f455a3e"), new Guid("80286c22-aa3f-457c-a308-f01542307c02") },
                    { new Guid("e3d1e155-8245-40ce-8820-158939f94d08"), new Guid("5eec99b0-5277-4d75-b2c4-8db0587495d1") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("12996b73-45c7-456f-bdbb-617c47786e1b"), new Guid("f21e5460-36b5-4fe1-b3c7-2d1b9f0c6694") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("16b3545f-ed63-426a-88e3-64a016c44540"), new Guid("e357ce88-65e5-413c-acb3-6b6a8ac35b08") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("3259a124-f4d3-4159-a195-7bed362c5b06"), new Guid("00cf06fa-8954-46c3-be43-2276b24864b4") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("403f6449-7b47-4505-9bc3-d7f057b8b562"), new Guid("5bd2218a-d12f-4489-a6f4-de90321c3289") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("448882ac-404c-4385-a47e-fa6f47c362a6"), new Guid("de341505-37ae-4dde-b647-7a2b7ae0094d") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("5cadaf82-f481-4d73-9fdf-46bcb6ab5039"), new Guid("a6f0a23f-0d19-4af4-b4a2-75525d82c53b") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("6c9def13-100e-492f-a464-773343a8d8b6"), new Guid("2ecb069d-56eb-4b12-848e-6fb847f0352a") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("7a1791c9-805d-48c0-a8a5-977d2563e602"), new Guid("303c1d09-c3ac-4953-a47b-58329bea4bcc") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("8c8ce09e-bc0c-4254-be44-c382607d1a34"), new Guid("a6bfca41-c56d-4d4d-a38c-016e94d8b841") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("9145acd3-4b9b-44fb-bdfb-131bc25ed151"), new Guid("6140c520-3b69-4019-923e-c2b402a05a96") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("92bab77b-a886-4e43-b62c-746c047b4810"), new Guid("5666ad20-8b6e-497f-aa65-5d74003b9fc3") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("9b599484-289e-4273-914a-95e2bf142bbf"), new Guid("3ba2f8b1-126d-4251-ae77-ea977be1304b") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("a1859120-8276-4ec7-934a-0d1e0a102507"), new Guid("2f0339c9-8371-45d2-a844-2d208468dad4") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("a6d8459a-ccc9-436e-a670-72f6f925f7ed"), new Guid("60a98bf7-169c-42ef-ba03-fad6e6494c30") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("b7e813a7-c8b3-4348-a6f3-32b21df00bb9"), new Guid("493d682f-7cbd-445a-a18f-2a2d05f55434") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("bb8111a3-b58d-403a-9245-63961f2f291b"), new Guid("ccb1504c-7ecb-4558-8634-5dacfdb1aec4") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("e31f5b2a-7cee-4731-a8f1-b5cc2f455a3e"), new Guid("80286c22-aa3f-457c-a308-f01542307c02") });

            migrationBuilder.DeleteData(
                table: "CourseScenarios",
                keyColumns: new[] { "CourseId", "ScenarioId" },
                keyValues: new object[] { new Guid("e3d1e155-8245-40ce-8820-158939f94d08"), new Guid("5eec99b0-5277-4d75-b2c4-8db0587495d1") });

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("12996b73-45c7-456f-bdbb-617c47786e1b"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("16b3545f-ed63-426a-88e3-64a016c44540"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("3259a124-f4d3-4159-a195-7bed362c5b06"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("403f6449-7b47-4505-9bc3-d7f057b8b562"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("448882ac-404c-4385-a47e-fa6f47c362a6"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("5cadaf82-f481-4d73-9fdf-46bcb6ab5039"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("6c9def13-100e-492f-a464-773343a8d8b6"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("7a1791c9-805d-48c0-a8a5-977d2563e602"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("8c8ce09e-bc0c-4254-be44-c382607d1a34"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("9145acd3-4b9b-44fb-bdfb-131bc25ed151"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("92bab77b-a886-4e43-b62c-746c047b4810"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("9b599484-289e-4273-914a-95e2bf142bbf"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("a1859120-8276-4ec7-934a-0d1e0a102507"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("a6d8459a-ccc9-436e-a670-72f6f925f7ed"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("b7e813a7-c8b3-4348-a6f3-32b21df00bb9"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("bb8111a3-b58d-403a-9245-63961f2f291b"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("e31f5b2a-7cee-4731-a8f1-b5cc2f455a3e"));

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: new Guid("e3d1e155-8245-40ce-8820-158939f94d08"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("00cf06fa-8954-46c3-be43-2276b24864b4"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("2ecb069d-56eb-4b12-848e-6fb847f0352a"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("2f0339c9-8371-45d2-a844-2d208468dad4"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("303c1d09-c3ac-4953-a47b-58329bea4bcc"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("3ba2f8b1-126d-4251-ae77-ea977be1304b"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("493d682f-7cbd-445a-a18f-2a2d05f55434"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("5666ad20-8b6e-497f-aa65-5d74003b9fc3"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("5bd2218a-d12f-4489-a6f4-de90321c3289"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("5eec99b0-5277-4d75-b2c4-8db0587495d1"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("60a98bf7-169c-42ef-ba03-fad6e6494c30"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("6140c520-3b69-4019-923e-c2b402a05a96"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("80286c22-aa3f-457c-a308-f01542307c02"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("a6bfca41-c56d-4d4d-a38c-016e94d8b841"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("a6f0a23f-0d19-4af4-b4a2-75525d82c53b"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("ccb1504c-7ecb-4558-8634-5dacfdb1aec4"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("de341505-37ae-4dde-b647-7a2b7ae0094d"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("e357ce88-65e5-413c-acb3-6b6a8ac35b08"));

            migrationBuilder.DeleteData(
                table: "TrainingScenarios",
                keyColumn: "Id",
                keyValue: new Guid("f21e5460-36b5-4fe1-b3c7-2d1b9f0c6694"));
        }
    }
}
