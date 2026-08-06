using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRAcademy.Api.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NameSr = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    DescriptionSr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ValidityMonths = table.Column<int>(type: "int", nullable: false),
                    PassScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingScenarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NameSr = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    DescriptionSr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RiskCategory = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingScenarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(128)", maxLength: 128, nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "varbinary(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseScenarios",
                columns: table => new
                {
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseScenarios", x => new { x.CourseId, x.ScenarioId });
                    table.ForeignKey(
                        name: "FK_CourseScenarios_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseScenarios_TrainingScenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "TrainingScenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthSessions",
                columns: table => new
                {
                    AccessToken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthSessions", x => x.AccessToken);
                    table.ForeignKey(
                        name: "FK_AuthSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CertificateNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    WorkerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ValidUntil = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    EnrolledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "Code", "DescriptionEn", "DescriptionSr", "NameEn", "NameSr", "PassScore", "ValidityMonths" },
                values: new object[,]
                {
                    { new Guid("1f331493-79d8-42c5-b3d0-8c0b11330f27"), "chemical-waste", "Classification, labeling, storage, and spill response.", "Klasifikacija, obelezavanje, skladistenje i reakcija na prosipanje.", "Chemical waste management", "Upravljanje hemijskim otpadom", 80, 12 },
                    { new Guid("30b016e2-cbe6-4fab-a6f5-6e3f47ee4cc8"), "biomedical-waste", "Separation, packaging, and temporary storage of healthcare waste.", "Razdvajanje, pakovanje i privremeno skladistenje zdravstvenog otpada.", "Biomedical waste management", "Upravljanje biomedicinskim otpadom", 80, 12 },
                    { new Guid("3a37e5bb-b46c-4d41-aeca-f5ad8a9c27e5"), "radioactive-materials", "Protective equipment, exposure control, and procedural checks.", "Zastitna oprema, kontrola izlozenosti i proceduralne provere.", "Radioactive material handling", "Upravljanje radioaktivnim materijalima", 80, 12 },
                    { new Guid("4cce62e1-7849-4a1e-93dd-796043ebd18c"), "electronic-waste", "Sorting devices, batteries, cables, and sensitive components.", "Sortiranje uredjaja, baterija, kablova i osetljivih komponenti.", "Electronic waste management", "Upravljanje elektronskim otpadom", 80, 12 },
                    { new Guid("79c6aa17-f058-4329-9fd5-8ec279cc873d"), "construction-waste", "Material sorting, safe work, and waste flow tracking.", "Razdvajanje materijala, bezbedan rad i pracenje tokova otpada.", "Construction waste management", "Upravljanje gradjevinskim otpadom", 80, 12 },
                    { new Guid("c3d3b41f-3017-4861-9d09-dbf349b1df43"), "fire-protection", "Risk recognition, evacuation, equipment selection, and correct response.", "Prepoznavanje rizika, evakuacija, izbor opreme i pravilno reagovanje.", "Fire protection", "Protivpozarna zastita", 80, 12 }
                });

            migrationBuilder.InsertData(
                table: "TrainingScenarios",
                columns: new[] { "Id", "Code", "DescriptionEn", "DescriptionSr", "EstimatedMinutes", "NameEn", "NameSr", "RiskCategory" },
                values: new object[,]
                {
                    { new Guid("0d91384d-6c32-4f84-89c6-2bc378d01c18"), "radioactive-materials", "Protective equipment, exposure control, and procedural checks.", "Zastitna oprema, kontrola izlozenosti i proceduralne provere.", 45, "Radioactive material handling", "Upravljanje radioaktivnim materijalima", "Hazardous materials" },
                    { new Guid("2a2d3685-1488-4d53-bd78-b82bb3a4c0d0"), "biomedical-waste", "Separation, packaging, and temporary storage of healthcare waste.", "Razdvajanje, pakovanje i privremeno skladistenje zdravstvenog otpada.", 40, "Biomedical waste management", "Upravljanje biomedicinskim otpadom", "Healthcare safety" },
                    { new Guid("bf90d0a8-f907-40fc-9450-bc6ec237bcb9"), "chemical-waste", "Classification, labeling, storage, and spill response.", "Klasifikacija, obelezavanje, skladistenje i reakcija na prosipanje.", 40, "Chemical waste management", "Upravljanje hemijskim otpadom", "Waste management" },
                    { new Guid("c391aa43-b3c6-4876-93db-fca11143e2b5"), "electronic-waste", "Sorting devices, batteries, cables, and sensitive components.", "Sortiranje uredjaja, baterija, kablova i osetljivih komponenti.", 35, "Electronic waste management", "Upravljanje elektronskim otpadom", "Waste management" },
                    { new Guid("c77fe939-98ee-4ca5-9f61-0d8f9d31f2b2"), "fire-protection", "Risk recognition, evacuation, equipment selection, and correct response.", "Prepoznavanje rizika, evakuacija, izbor opreme i pravilno reagovanje.", 35, "Fire protection", "Protivpozarna zastita", "Safety" },
                    { new Guid("ffdb0364-47e2-4f4d-96b3-38675ba0938a"), "construction-waste", "Material sorting, safe work, and waste flow tracking.", "Razdvajanje materijala, bezbedan rad i pracenje tokova otpada.", 30, "Construction waste management", "Upravljanje gradjevinskim otpadom", "Waste management" }
                });

            migrationBuilder.InsertData(
                table: "CourseScenarios",
                columns: new[] { "CourseId", "ScenarioId" },
                values: new object[,]
                {
                    { new Guid("1f331493-79d8-42c5-b3d0-8c0b11330f27"), new Guid("bf90d0a8-f907-40fc-9450-bc6ec237bcb9") },
                    { new Guid("30b016e2-cbe6-4fab-a6f5-6e3f47ee4cc8"), new Guid("2a2d3685-1488-4d53-bd78-b82bb3a4c0d0") },
                    { new Guid("3a37e5bb-b46c-4d41-aeca-f5ad8a9c27e5"), new Guid("0d91384d-6c32-4f84-89c6-2bc378d01c18") },
                    { new Guid("4cce62e1-7849-4a1e-93dd-796043ebd18c"), new Guid("c391aa43-b3c6-4876-93db-fca11143e2b5") },
                    { new Guid("79c6aa17-f058-4329-9fd5-8ec279cc873d"), new Guid("ffdb0364-47e2-4f4d-96b3-38675ba0938a") },
                    { new Guid("c3d3b41f-3017-4861-9d09-dbf349b1df43"), new Guid("c77fe939-98ee-4ca5-9f61-0d8f9d31f2b2") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthSessions_UserId",
                table: "AuthSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CertificateNumber",
                table: "Certificates",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CourseId",
                table: "Certificates",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_WorkerId",
                table: "Certificates",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Code",
                table: "Courses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseScenarios_ScenarioId",
                table: "CourseScenarios",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_WorkerId",
                table: "Enrollments",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingScenarios_Code",
                table: "TrainingScenarios",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workers_CompanyId_EmployeeNumber",
                table: "Workers",
                columns: new[] { "CompanyId", "EmployeeNumber" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthSessions");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "CourseScenarios");

            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "TrainingScenarios");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Workers");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
