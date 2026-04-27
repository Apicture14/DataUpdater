using System;
using System.Linq;
using System.Threading.Tasks;
using DataUpdater.Core;
using DataUpdater.DAO;
using DataUpdater.Models;

namespace DataUpdater.Ext;

public static class Extensions
{
    public static async Task<Model.Arrival> FromArrivalPack(this Model.ArrivalPack ap,DataAccessor dataAccessor = null)
    {
        return new Model.Arrival()
        {
            arrivalTime = DateTimeOffset.FromUnixTimeMilliseconds(ap.arrival).ToLocalTime(),
            departureTime = DateTimeOffset.FromUnixTimeMilliseconds(ap.departure).ToLocalTime(),
            destination = (dataAccessor != null)
                ? (await Fetcher.Name2StationObject(dataAccessor, ap.destination.Split("|")[0])).FirstOrDefault()
                : null,
            destinationName = ap.destination,
            isTerminating = ap.isTerminating,
            isScheduled = ap.realTime,
            platformName = ap.platformName,
            routeFrom = (dataAccessor != null) ? (await Fetcher.Name2RouteObject(dataAccessor, ap.routeName)).FirstOrDefault() : null
        };
    }
}