using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TransactionWorkflowEngine.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransactionStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsInitial = table.Column<bool>(type: "bit", nullable: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_TransactionStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "TransactionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionStatusTransitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromStatusId = table.Column<int>(type: "int", nullable: false),
                    ToStatusId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequiresComment = table.Column<bool>(type: "bit", nullable: false),
                    IsRollback = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionStatusTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionStatusTransitions_TransactionStatuses_FromStatusId",
                        column: x => x.FromStatusId,
                        principalTable: "TransactionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionStatusTransitions_TransactionStatuses_ToStatusId",
                        column: x => x.ToStatusId,
                        principalTable: "TransactionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatusId = table.Column<int>(type: "int", nullable: false),
                    ToStatusId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionHistories_TransactionStatuses_FromStatusId",
                        column: x => x.FromStatusId,
                        principalTable: "TransactionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionHistories_TransactionStatuses_ToStatusId",
                        column: x => x.ToStatusId,
                        principalTable: "TransactionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionHistories_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TransactionStatuses",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "IsFinal", "IsInitial", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 12, 10, 20, 27, 932, DateTimeKind.Utc).AddTicks(8673), "Transaction has been created", 1, false, true, "Created" },
                    { 2, new DateTime(2026, 2, 12, 10, 20, 27, 932, DateTimeKind.Utc).AddTicks(9886), "Transaction has been validated", 2, false, false, "Validated" },
                    { 3, new DateTime(2026, 2, 12, 10, 20, 27, 932, DateTimeKind.Utc).AddTicks(9889), "Transaction is being processed", 3, false, false, "Processing" },
                    { 4, new DateTime(2026, 2, 12, 10, 20, 27, 932, DateTimeKind.Utc).AddTicks(9891), "Transaction has been completed", 4, true, false, "Completed" },
                    { 5, new DateTime(2026, 2, 12, 10, 20, 27, 932, DateTimeKind.Utc).AddTicks(9893), "Transaction processing failed", 5, false, false, "Failed" }
                });

            migrationBuilder.InsertData(
                table: "TransactionStatusTransitions",
                columns: new[] { "Id", "CreatedAt", "Description", "FromStatusId", "IsRollback", "Name", "RequiresComment", "ToStatusId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 12, 10, 20, 27, 933, DateTimeKind.Utc).AddTicks(5028), "Validate the created transaction", 1, false, "Validate", false, 2 },
                    { 2, new DateTime(2026, 2, 12, 10, 20, 27, 933, DateTimeKind.Utc).AddTicks(5961), "Begin processing the validated transaction", 2, false, "Start Processing", false, 3 },
                    { 3, new DateTime(2026, 2, 12, 10, 20, 27, 933, DateTimeKind.Utc).AddTicks(5964), "Mark transaction as completed", 3, false, "Complete", false, 4 },
                    { 4, new DateTime(2026, 2, 12, 10, 20, 27, 933, DateTimeKind.Utc).AddTicks(5965), "Mark transaction as failed", 3, false, "Fail", true, 5 },
                    { 5, new DateTime(2026, 2, 12, 10, 20, 27, 933, DateTimeKind.Utc).AddTicks(6118), "Retry the failed transaction", 5, true, "Retry", false, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistories_ChangedAt",
                table: "TransactionHistories",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistories_FromStatusId",
                table: "TransactionHistories",
                column: "FromStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistories_ToStatusId",
                table: "TransactionHistories",
                column: "ToStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistories_TransactionId",
                table: "TransactionHistories",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CustomerId",
                table: "Transactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReferenceNumber",
                table: "Transactions",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_StatusId",
                table: "Transactions",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionStatuses_Name",
                table: "TransactionStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionStatusTransitions_FromStatusId_ToStatusId",
                table: "TransactionStatusTransitions",
                columns: new[] { "FromStatusId", "ToStatusId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionStatusTransitions_ToStatusId",
                table: "TransactionStatusTransitions",
                column: "ToStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionHistories");

            migrationBuilder.DropTable(
                name: "TransactionStatusTransitions");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "TransactionStatuses");
        }
    }
}
