using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Infrastructure.Persistence.Seed;

public static class ShiftTemplateSeedData
{
    public static IReadOnlyList<ShiftTemplate> Build(DateTime seedTime)
    {
        var siteId = IdentitySeedData.CxrSiteId;
        const string dept = "PVHK_DI";

        return
        [
            ShiftTemplate.Create(siteId, dept, "HC-S", "Hành chính sáng", new TimeOnly(6, 15), new TimeOnly(16, 30), false, 8m, OperationalSegment.Qn, 1, seedTime),
            ShiftTemplate.Create(siteId, dept, "HC-C", "Hành chính chiều", new TimeOnly(8, 0), new TimeOnly(18, 0), false, 8m, null, 2, seedTime),
            ShiftTemplate.Create(siteId, dept, "CD", "Ca đêm", new TimeOnly(14, 0), new TimeOnly(1, 0), true, 8m, OperationalSegment.Qt, 3, seedTime),
            ShiftTemplate.Create(siteId, dept, "SG-S", "Sáng sớm", new TimeOnly(4, 30), new TimeOnly(13, 0), false, 8m, OperationalSegment.Qn, 4, seedTime),
            ShiftTemplate.Create(siteId, dept, "HC-8", "HC 8 tiếng", new TimeOnly(8, 0), new TimeOnly(17, 0), false, 8m, null, 5, seedTime),
        ];
    }
}
