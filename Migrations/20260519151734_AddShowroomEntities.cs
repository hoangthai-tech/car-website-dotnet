using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddShowroomEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<long>(
                name: "DepositAmount",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleUnitId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BankLoans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoanAmount = table.Column<long>(type: "bigint", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoanYears = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DisbursedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankLoans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankLoans_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankLoans_Users_ConfirmedById",
                        column: x => x.ConfirmedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Commissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commissions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Commissions_Users_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterestedCarModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerProfiles_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiptCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    PaymentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConfirmedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentReceipts_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentReceipts_Users_ConfirmedById",
                        column: x => x.ConfirmedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentVouchers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Recipient = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentVouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentVouchers_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServiceTickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Odometer = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedTechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LaborCost = table.Column<long>(type: "bigint", nullable: false),
                    TotalPartsCost = table.Column<long>(type: "bigint", nullable: false),
                    TotalAmount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceTickets_Users_AssignedTechnicianId",
                        column: x => x.AssignedTechnicianId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceTickets_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SpareParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    MinStock = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<long>(type: "bigint", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpareParts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestDrives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CarId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestDrives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestDrives_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestDrives_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestDrives_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TodoItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedToId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoItems_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Vin = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EngineNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CarId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExteriorColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InteriorColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PurchasePrice = table.Column<long>(type: "bigint", nullable: false),
                    ListPrice = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleUnits_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleUnits_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CustomerNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerNotes_CustomerProfiles_CustomerProfileId",
                        column: x => x.CustomerProfileId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomerNotes_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomerNotes_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SparePartUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceTicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SparePartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SparePartUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SparePartUsages_ServiceTickets_ServiceTicketId",
                        column: x => x.ServiceTicketId,
                        principalTable: "ServiceTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SparePartUsages_SpareParts_SparePartId",
                        column: x => x.SparePartId,
                        principalTable: "SpareParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PdiChecklists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExteriorPassed = table.Column<bool>(type: "bit", nullable: false),
                    InteriorPassed = table.Column<bool>(type: "bit", nullable: false),
                    ElectricalPassed = table.Column<bool>(type: "bit", nullable: false),
                    EnginePassed = table.Column<bool>(type: "bit", nullable: false),
                    TirePassed = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InspectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdiChecklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PdiChecklists_Users_InspectorId",
                        column: x => x.InspectorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PdiChecklists_VehicleUnits_VehicleUnitId",
                        column: x => x.VehicleUnitId,
                        principalTable: "VehicleUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreDeliveryOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstructionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedToId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreDeliveryOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreDeliveryOrders_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreDeliveryOrders_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PreDeliveryOrders_VehicleUnits_VehicleUnitId",
                        column: x => x.VehicleUnitId,
                        principalTable: "VehicleUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PdiDefects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PdiChecklistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdiDefects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PdiDefects_PdiChecklists_PdiChecklistId",
                        column: x => x.PdiChecklistId,
                        principalTable: "PdiChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_VehicleUnitId",
                table: "Orders",
                column: "VehicleUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BankLoans_ConfirmedById",
                table: "BankLoans",
                column: "ConfirmedById");

            migrationBuilder.CreateIndex(
                name: "IX_BankLoans_OrderId",
                table: "BankLoans",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_OrderId",
                table: "Commissions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_StaffId",
                table: "Commissions",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotes_CreatedById",
                table: "CustomerNotes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotes_CustomerId",
                table: "CustomerNotes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotes_CustomerProfileId",
                table: "CustomerNotes",
                column: "CustomerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfiles_CustomerId",
                table: "CustomerProfiles",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_ConfirmedById",
                table: "PaymentReceipts",
                column: "ConfirmedById");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_OrderId",
                table: "PaymentReceipts",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_ReceiptCode",
                table: "PaymentReceipts",
                column: "ReceiptCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVouchers_CreatedById",
                table: "PaymentVouchers",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVouchers_VoucherCode",
                table: "PaymentVouchers",
                column: "VoucherCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PdiChecklists_InspectorId",
                table: "PdiChecklists",
                column: "InspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_PdiChecklists_VehicleUnitId",
                table: "PdiChecklists",
                column: "VehicleUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PdiDefects_PdiChecklistId",
                table: "PdiDefects",
                column: "PdiChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_PreDeliveryOrders_AssignedToId",
                table: "PreDeliveryOrders",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_PreDeliveryOrders_OrderId",
                table: "PreDeliveryOrders",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PreDeliveryOrders_VehicleUnitId",
                table: "PreDeliveryOrders",
                column: "VehicleUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTickets_AssignedTechnicianId",
                table: "ServiceTickets",
                column: "AssignedTechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTickets_CreatedById",
                table: "ServiceTickets",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTickets_TicketCode",
                table: "ServiceTickets",
                column: "TicketCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpareParts_PartCode",
                table: "SpareParts",
                column: "PartCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SparePartUsages_ServiceTicketId",
                table: "SparePartUsages",
                column: "ServiceTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_SparePartUsages_SparePartId",
                table: "SparePartUsages",
                column: "SparePartId");

            migrationBuilder.CreateIndex(
                name: "IX_TestDrives_CarId",
                table: "TestDrives",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_TestDrives_CreatedById",
                table: "TestDrives",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TestDrives_CustomerId",
                table: "TestDrives",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_AssignedToId",
                table: "TodoItems",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleUnits_CarId",
                table: "VehicleUnits",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleUnits_CreatedById",
                table: "VehicleUnits",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleUnits_Vin",
                table: "VehicleUnits",
                column: "Vin",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_VehicleUnits_VehicleUnitId",
                table: "Orders",
                column: "VehicleUnitId",
                principalTable: "VehicleUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_VehicleUnits_VehicleUnitId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "BankLoans");

            migrationBuilder.DropTable(
                name: "Commissions");

            migrationBuilder.DropTable(
                name: "CustomerNotes");

            migrationBuilder.DropTable(
                name: "PaymentReceipts");

            migrationBuilder.DropTable(
                name: "PaymentVouchers");

            migrationBuilder.DropTable(
                name: "PdiDefects");

            migrationBuilder.DropTable(
                name: "PreDeliveryOrders");

            migrationBuilder.DropTable(
                name: "SparePartUsages");

            migrationBuilder.DropTable(
                name: "TestDrives");

            migrationBuilder.DropTable(
                name: "TodoItems");

            migrationBuilder.DropTable(
                name: "CustomerProfiles");

            migrationBuilder.DropTable(
                name: "PdiChecklists");

            migrationBuilder.DropTable(
                name: "ServiceTickets");

            migrationBuilder.DropTable(
                name: "SpareParts");

            migrationBuilder.DropTable(
                name: "VehicleUnits");

            migrationBuilder.DropIndex(
                name: "IX_Orders_VehicleUnitId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DepositAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VehicleUnitId",
                table: "Orders");
        }
    }
}
