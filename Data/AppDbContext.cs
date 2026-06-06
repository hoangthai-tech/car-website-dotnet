using Microsoft.EntityFrameworkCore;
using CarWebsite.Models;

namespace CarWebsite.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Existing
    public DbSet<User> Users => Set<User>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<DiscountRequest> DiscountRequests => Set<DiscountRequest>();
    public DbSet<News> News => Set<News>();

    // Kho
    public DbSet<VehicleUnit> VehicleUnits => Set<VehicleUnit>();
    public DbSet<PdiChecklist> PdiChecklists => Set<PdiChecklist>();
    public DbSet<PdiDefect> PdiDefects => Set<PdiDefect>();
    public DbSet<PreDeliveryOrder> PreDeliveryOrders => Set<PreDeliveryOrder>();

    // Kinh doanh / CRM
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<CustomerNote> CustomerNotes => Set<CustomerNote>();
    public DbSet<TestDrive> TestDrives => Set<TestDrive>();

    // Kế toán
    public DbSet<PaymentReceipt> PaymentReceipts => Set<PaymentReceipt>();
    public DbSet<BankLoan> BankLoans => Set<BankLoan>();
    public DbSet<PaymentVoucher> PaymentVouchers => Set<PaymentVoucher>();
    public DbSet<Commission> Commissions => Set<Commission>();

    // Kỹ thuật & Hậu mãi
    public DbSet<ServiceTicket> ServiceTickets => Set<ServiceTicket>();
    public DbSet<SparePart> SpareParts => Set<SparePart>();
    public DbSet<SparePartUsage> SparePartUsages => Set<SparePartUsage>();

    // Chính sách & Điều khoản
    public DbSet<TermsOfService> TermsOfServices => Set<TermsOfService>();
    public DbSet<UserTermAgreement> UserTermAgreements => Set<UserTermAgreement>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── User ─────────────────────────────────────────────────────────────
        builder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasMaxLength(20);
        });

        // ── Car ──────────────────────────────────────────────────────────────
        builder.Entity<Car>(e =>
        {
            e.HasIndex(c => c.Slug).IsUnique();
            e.Property(c => c.Type).HasMaxLength(20);
            e.Property(c => c.Fuel).HasMaxLength(10);
            e.Property(c => c.Status).HasMaxLength(20);
        });

        // ── Order ────────────────────────────────────────────────────────────
        builder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.OrderCode).IsUnique();
            e.HasOne(o => o.Customer).WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<Car>().WithMany()
                .HasForeignKey(o => o.CarId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<VehicleUnit>().WithMany()
                .HasForeignKey(o => o.VehicleUnitId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<User>().WithMany()
                .HasForeignKey(o => o.StaffId).OnDelete(DeleteBehavior.NoAction);
        });

        // ── DiscountRequest ───────────────────────────────────────────────────
        builder.Entity<DiscountRequest>(e =>
        {
            e.HasOne<Order>().WithMany().HasForeignKey(d => d.OrderId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<User>().WithMany().HasForeignKey(d => d.StaffId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<User>().WithMany().HasForeignKey(d => d.ReviewedById).OnDelete(DeleteBehavior.NoAction);
        });

        // ── VehicleUnit ──────────────────────────────────────────────────────
        builder.Entity<VehicleUnit>(e =>
        {
            e.HasIndex(v => v.Vin).IsUnique();
            e.Property(v => v.Status).HasMaxLength(20);
            e.HasOne(v => v.Car).WithMany()
                .HasForeignKey(v => v.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.CreatedBy).WithMany()
                .HasForeignKey(v => v.CreatedById).OnDelete(DeleteBehavior.SetNull);
        });

        // ── PdiChecklist ─────────────────────────────────────────────────────
        builder.Entity<PdiChecklist>(e =>
        {
            e.HasOne(p => p.VehicleUnit).WithMany(v => v.PdiChecklists)
                .HasForeignKey(p => p.VehicleUnitId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.Inspector).WithMany()
                .HasForeignKey(p => p.InspectorId).OnDelete(DeleteBehavior.NoAction);
        });

        // ── PdiDefect ────────────────────────────────────────────────────────
        builder.Entity<PdiDefect>(e =>
        {
            e.HasOne(d => d.PdiChecklist).WithMany(p => p.Defects)
                .HasForeignKey(d => d.PdiChecklistId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── PreDeliveryOrder ─────────────────────────────────────────────────
        builder.Entity<PreDeliveryOrder>(e =>
        {
            e.HasOne(p => p.Order).WithMany()
                .HasForeignKey(p => p.OrderId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.VehicleUnit).WithMany()
                .HasForeignKey(p => p.VehicleUnitId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.AssignedTo).WithMany()
                .HasForeignKey(p => p.AssignedToId).OnDelete(DeleteBehavior.SetNull);
        });

        // ── TodoItem ─────────────────────────────────────────────────────────
        builder.Entity<TodoItem>(e =>
        {
            e.Property(t => t.Status).HasMaxLength(20);
            e.HasOne(t => t.AssignedTo).WithMany()
                .HasForeignKey(t => t.AssignedToId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── CustomerProfile ──────────────────────────────────────────────────
        builder.Entity<CustomerProfile>(e =>
        {
            e.HasIndex(c => c.CustomerId).IsUnique();
            e.HasOne(c => c.Customer).WithMany()
                .HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── CustomerNote ─────────────────────────────────────────────────────
        builder.Entity<CustomerNote>(e =>
        {
            e.HasOne(n => n.Customer).WithMany()
                .HasForeignKey(n => n.CustomerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(n => n.CreatedBy).WithMany()
                .HasForeignKey(n => n.CreatedById).OnDelete(DeleteBehavior.NoAction);
        });

        // ── TestDrive ────────────────────────────────────────────────────────
        builder.Entity<TestDrive>(e =>
        {
            e.Property(t => t.Status).HasMaxLength(20);
            e.HasOne(t => t.Customer).WithMany()
                .HasForeignKey(t => t.CustomerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.Car).WithMany()
                .HasForeignKey(t => t.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.CreatedBy).WithMany()
                .HasForeignKey(t => t.CreatedById).OnDelete(DeleteBehavior.NoAction);
        });

        // ── PaymentReceipt ───────────────────────────────────────────────────
        builder.Entity<PaymentReceipt>(e =>
        {
            e.HasIndex(r => r.ReceiptCode).IsUnique();
            e.Property(r => r.PaymentType).HasMaxLength(20);
            e.HasOne(r => r.Order).WithMany()
                .HasForeignKey(r => r.OrderId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.ConfirmedBy).WithMany()
                .HasForeignKey(r => r.ConfirmedById).OnDelete(DeleteBehavior.NoAction);
        });

        // ── BankLoan ─────────────────────────────────────────────────────────
        builder.Entity<BankLoan>(e =>
        {
            e.Property(b => b.Status).HasMaxLength(20);
            e.Property(b => b.InterestRate).HasPrecision(5, 2);
            e.HasOne(b => b.Order).WithMany()
                .HasForeignKey(b => b.OrderId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.ConfirmedBy).WithMany()
                .HasForeignKey(b => b.ConfirmedById).OnDelete(DeleteBehavior.SetNull);
        });

        // ── PaymentVoucher ───────────────────────────────────────────────────
        builder.Entity<PaymentVoucher>(e =>
        {
            e.HasIndex(v => v.VoucherCode).IsUnique();
            e.Property(v => v.Category).HasMaxLength(30);
            e.HasOne(v => v.CreatedBy).WithMany()
                .HasForeignKey(v => v.CreatedById).OnDelete(DeleteBehavior.NoAction);
        });

        // ── Commission ───────────────────────────────────────────────────────
        builder.Entity<Commission>(e =>
        {
            e.Property(c => c.Rate).HasPrecision(5, 4);
            e.HasOne(c => c.Staff).WithMany()
                .HasForeignKey(c => c.StaffId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Order).WithMany()
                .HasForeignKey(c => c.OrderId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── ServiceTicket ────────────────────────────────────────────────────
        builder.Entity<ServiceTicket>(e =>
        {
            e.HasIndex(s => s.TicketCode).IsUnique();
            e.Property(s => s.Status).HasMaxLength(20);
            e.HasOne(s => s.AssignedTechnician).WithMany()
                .HasForeignKey(s => s.AssignedTechnicianId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(s => s.CreatedBy).WithMany()
                .HasForeignKey(s => s.CreatedById).OnDelete(DeleteBehavior.NoAction);
        });

        // ── SparePart ────────────────────────────────────────────────────────
        builder.Entity<SparePart>(e =>
        {
            e.HasIndex(s => s.PartCode).IsUnique();
        });

        // ── SparePartUsage ───────────────────────────────────────────────────
        builder.Entity<SparePartUsage>(e =>
        {
            e.HasOne(u => u.ServiceTicket).WithMany(s => s.SparePartUsages)
                .HasForeignKey(u => u.ServiceTicketId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(u => u.SparePart).WithMany(s => s.Usages)
                .HasForeignKey(u => u.SparePartId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── TermsOfService ───────────────────────────────────────────────────
        builder.Entity<TermsOfService>(e =>
        {
            e.Property(t => t.Version).HasMaxLength(20);
            e.HasOne(t => t.PublishedBy).WithMany()
                .HasForeignKey(t => t.PublishedById).OnDelete(DeleteBehavior.SetNull);
        });

        // ── UserTermAgreement ────────────────────────────────────────────────
        builder.Entity<UserTermAgreement>(e =>
        {
            e.HasOne(a => a.User).WithMany()
                .HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.TermsOfService).WithMany(t => t.Agreements)
                .HasForeignKey(a => a.TermsOfServiceId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => new { a.UserId, a.TermsOfServiceId }).IsUnique();
        });
    }
}
