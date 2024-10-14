using Merken.Core.Enums;
using Merken.Core.Models.Data;
using Merken.Core.Models.Fsrs;

namespace Merken.Core.Services.Abstractions;

public interface IFsrsService
{
    Dictionary<Rating, SchedulingInfo> Repeat(Card card);
}