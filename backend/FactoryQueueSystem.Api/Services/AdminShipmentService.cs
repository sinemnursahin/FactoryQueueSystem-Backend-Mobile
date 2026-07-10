using FactoryQueueSystem.Api.Data;
using FactoryQueueSystem.Api.DTOs.Admin;
using FactoryQueueSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FactoryQueueSystem.Api.Services;

public class AdminShipmentService(AppDbContext db, FactoryClock clock)
{
    public async Task<List<AdminShipmentResponse>> GetQueueAsync() =>
        await Query().Where(x => x.Status == ShipmentStatus.InQueue)
            .OrderBy(x => x.QueueDate).ThenBy(x => x.QueueNumber)
            .Select(x => ToAdminResponse(x))
            .ToListAsync();

    public async Task<List<AdminShipmentResponse>> GetActiveAsync() =>
        await Query().Where(x => x.Status != ShipmentStatus.Completed)
            .OrderBy(x => x.QueueDate == null).ThenBy(x => x.QueueDate).ThenBy(x => x.QueueNumber).ThenByDescending(x => x.CreatedAt)
            .Select(x => ToAdminResponse(x))
            .ToListAsync();

    public async Task<List<AdminShipmentResponse>> GetByStatusAsync(ShipmentStatus status) =>
        await Query().Where(x => x.Status == status)
            .OrderBy(x => x.QueueDate).ThenBy(x => x.QueueNumber)
            .Select(x => ToAdminResponse(x))
            .ToListAsync();

    public async Task<List<AdminShipmentResponse>> GetCompletedAsync() =>
        await Query().Where(x => x.Status == ShipmentStatus.Completed)
            .OrderByDescending(x => x.CompletedAt)
            .Select(x => ToAdminResponse(x))
            .ToListAsync();

    public async Task<ServiceResult<AdminShipmentResponse>> CallToScaleAsync(Guid id) =>
        await CallToScaleOrderedAsync(id);

    public async Task<ServiceResult<AdminShipmentResponse>> StartUnloadingAsync(Guid id) =>
        await TransitionAsync(id, ShipmentStatus.AtScale, ShipmentStatus.Unloading);

    public async Task<ServiceResult<AdminShipmentResponse>> CompleteUnloadingAsync(Guid id) =>
        await TransitionAsync(id, ShipmentStatus.Unloading, ShipmentStatus.UnloadingComplete);

    public async Task<ServiceResult<AdminShipmentResponse>> SetLoadedWeightAsync(Guid id, decimal weight)
    {
        if (weight <= 0)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest("Dolu tartım sıfırdan büyük olmalıdır.");
        }

        var shipment = await Query().FirstOrDefaultAsync(x => x.Id == id);
        if (shipment == null)
        {
            return ServiceResult<AdminShipmentResponse>.NotFound("Sevkiyat bulunamadı.");
        }

        if (shipment.Status != ShipmentStatus.CalledToScale)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest("Dolu tartım sadece araç kantara çağrıldıktan sonra girilebilir.");
        }

        var weighingRecord = shipment.WeighingRecord;
        if (weighingRecord == null)
        {
            weighingRecord = new WeighingRecord
            {
                Id = Guid.NewGuid(),
                ShipmentId = shipment.Id
            };
            db.WeighingRecords.Add(weighingRecord);
            shipment.WeighingRecord = weighingRecord;
        }

        weighingRecord.LoadedWeight = weight;
        weighingRecord.LoadedWeighDate = clock.UtcNow;
        shipment.Status = ShipmentStatus.AtScale;

        await db.SaveChangesAsync();
        return ServiceResult<AdminShipmentResponse>.Ok(ToAdminResponse(shipment));
    }

    public async Task<ServiceResult<AdminShipmentResponse>> SetEmptyWeightAsync(Guid id, decimal weight)
    {
        if (weight < 0)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest("Boş tartım negatif olamaz.");
        }

        var shipment = await Query().FirstOrDefaultAsync(x => x.Id == id);
        if (shipment == null)
        {
            return ServiceResult<AdminShipmentResponse>.NotFound("Sevkiyat bulunamadı.");
        }

        if (shipment.Status == ShipmentStatus.Completed)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest("Tamamlanan sevkiyatlar değiştirilemez.");
        }

        if (shipment.Status != ShipmentStatus.UnloadingComplete)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest("Boş tartım sadece boşaltım tamamlandıktan sonra girilebilir.");
        }

        if (shipment.WeighingRecord?.LoadedWeight == null)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest("Boş tartımdan önce dolu tartım girilmelidir.");
        }

        if (weight > shipment.WeighingRecord.LoadedWeight.Value)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest("Boş tartım dolu tartımdan büyük olamaz.");
        }

        var now = clock.UtcNow;
        shipment.WeighingRecord.EmptyWeight = weight;
        shipment.WeighingRecord.EmptyWeighDate = now;
        shipment.WeighingRecord.NetAmount = shipment.WeighingRecord.LoadedWeight.Value - weight;
        shipment.Status = ShipmentStatus.Completed;
        shipment.CompletedAt = now;

        await db.SaveChangesAsync();
        return ServiceResult<AdminShipmentResponse>.Ok(ToAdminResponse(shipment));
    }

    private async Task<ServiceResult<AdminShipmentResponse>> TransitionAsync(Guid id, ShipmentStatus required, ShipmentStatus next)
    {
        var shipment = await Query().FirstOrDefaultAsync(x => x.Id == id);
        if (shipment == null)
        {
            return ServiceResult<AdminShipmentResponse>.NotFound("Sevkiyat bulunamadı.");
        }

        if (shipment.Status == ShipmentStatus.Completed)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest("Tamamlanan sevkiyatlar değiştirilemez.");
        }

        if (shipment.Status != required)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest($"Geçersiz durum geçişi: {StatusText(shipment.Status)} -> {StatusText(next)}.");
        }

        shipment.Status = next;
        await db.SaveChangesAsync();
        return ServiceResult<AdminShipmentResponse>.Ok(ToAdminResponse(shipment));
    }

    private async Task<ServiceResult<AdminShipmentResponse>> CallToScaleOrderedAsync(Guid id)
    {
        var shipment = await Query().FirstOrDefaultAsync(x => x.Id == id);
        if (shipment == null)
        {
            return ServiceResult<AdminShipmentResponse>.NotFound("Sevkiyat bulunamadı.");
        }

        if (shipment.Status == ShipmentStatus.Completed)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest("Tamamlanan sevkiyatlar değiştirilemez.");
        }

        if (shipment.Status != ShipmentStatus.InQueue)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest($"Geçersiz durum geçişi: {StatusText(shipment.Status)} -> {StatusText(ShipmentStatus.CalledToScale)}.");
        }

        if (shipment.QueueDate == null || shipment.QueueNumber == null)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest("Kantara çağrı için sıra bilgisi bulunamadı.");
        }

        var nextQueueNumber = await db.Shipments
            .Where(x => x.QueueDate == shipment.QueueDate && x.Status == ShipmentStatus.InQueue && x.QueueNumber != null)
            .MinAsync(x => (int?)x.QueueNumber);

        if (nextQueueNumber != shipment.QueueNumber)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest("Sıradaki araç beklemeden kantara çağrılamaz.");
        }

        var scaleBusy = await db.Shipments.AnyAsync(x =>
            x.QueueDate == shipment.QueueDate &&
            x.Id != shipment.Id &&
            (x.Status == ShipmentStatus.CalledToScale || x.Status == ShipmentStatus.AtScale));

        if (scaleBusy)
        {
            return ServiceResult<AdminShipmentResponse>.BadRequest("Kantarda başka bir araç işlem görüyor.");
        }

        shipment.Status = ShipmentStatus.CalledToScale;
        await db.SaveChangesAsync();
        return ServiceResult<AdminShipmentResponse>.Ok(ToAdminResponse(shipment));
    }

    private IQueryable<Shipment> Query() =>
        db.Shipments.IgnoreQueryFilters().Include(x => x.Vehicle).Include(x => x.WeighingRecord);

    private static AdminShipmentResponse ToAdminResponse(Shipment shipment) =>
        new(
            shipment.Id,
            shipment.VehicleId,
            shipment.Vehicle?.PlateNumber,
            shipment.Vehicle?.DriverName,
            (int)shipment.Status,
            StatusText(shipment.Status),
            shipment.QueueNumber,
            shipment.QueueDate,
            shipment.RawMaterialName,
            shipment.SupplierName,
            shipment.QueuedAt,
            shipment.WeighingRecord?.LoadedWeight,
            shipment.WeighingRecord?.EmptyWeight,
            shipment.WeighingRecord?.NetAmount,
            shipment.WeighingRecord?.LoadedWeighDate,
            shipment.WeighingRecord?.EmptyWeighDate,
            shipment.CompletedAt);

    private static string StatusText(ShipmentStatus status) => status switch
    {
        ShipmentStatus.OnTheWay => "Yolda",
        ShipmentStatus.InQueue => "Sırada",
        ShipmentStatus.CalledToScale => "Kantara Çağrıldı",
        ShipmentStatus.AtScale => "Kantarda",
        ShipmentStatus.Unloading => "Boşaltımda",
        ShipmentStatus.UnloadingComplete => "Boşaltım Tamamlandı",
        ShipmentStatus.Completed => "Tamamlandı",
        _ => status.ToString()
    };
}
