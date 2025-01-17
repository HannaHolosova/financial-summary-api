using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using FinancialSummaryApi.V1.Domain;
using FinancialSummaryApi.V1.Factories;
using FinancialSummaryApi.V1.Gateways.Abstracts;
using FinancialSummaryApi.V1.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialSummaryApi.V1.Gateways
{
    public class DynamoDbGateway : IFinanceSummaryGateway
    {
        private readonly DynamoDbContextWrapper _wrapper;
        private readonly IDynamoDBContext _dynamoDbContext;

        public DynamoDbGateway(IDynamoDBContext dynamoDbContext,
            DynamoDbContextWrapper wrapper)
        {
            _dynamoDbContext = dynamoDbContext;

            // Hanna Holasava
            // We need this wrapper only for unit tests purposes.
            // If you will find other way to test this, please contact me!
            _wrapper = wrapper;
        }

        #region Asset Summary

        public async Task AddAsync(AssetSummary assetSummary)
        {
            await _dynamoDbContext.SaveAsync(assetSummary.ToDatabase()).ConfigureAwait(false);
        }

        public async Task<List<AssetSummary>> GetAllAssetSummaryAsync(DateTime submitDate)
        {
            List<ScanCondition> scanConditions = new List<ScanCondition>();

            scanConditions.Add(new ScanCondition("SubmitDate", ScanOperator.Between, GetDayRange(submitDate).Item1, GetDayRange(submitDate).Item2));
            scanConditions.Add(new ScanCondition("TargetType", ScanOperator.In, TargetType.Estate, TargetType.Block, TargetType.Core));

            List<FinanceSummaryDbEntity> data = await _wrapper.ScanAsync(_dynamoDbContext, scanConditions).ConfigureAwait(false);

            return data.Select(s => s.ToAssetDomain()).OrderByDescending(r => r.SubmitDate).ToList();
        }

        public async Task<AssetSummary> GetAssetSummaryByIdAsync(Guid assetId, DateTime submitDate)
        {
            List<ScanCondition> scanConditions = new List<ScanCondition>();

            scanConditions.Add(new ScanCondition("SubmitDate", ScanOperator.Between, GetDayRange(submitDate).Item1, GetDayRange(submitDate).Item2));
            scanConditions.Add(new ScanCondition("TargetId", ScanOperator.Equal, assetId));
            scanConditions.Add(new ScanCondition("TargetType", ScanOperator.In, TargetType.Estate, TargetType.Block, TargetType.Core));

            List<FinanceSummaryDbEntity> data = await _wrapper.ScanAsync(_dynamoDbContext, scanConditions).ConfigureAwait(false);

            return data.OrderByDescending(r => r.SubmitDate).FirstOrDefault()?.ToAssetDomain();
        }

        #endregion

        #region Rent Group Summary

        public async Task AddAsync(RentGroupSummary rentGroupSummary)
        {
            await _dynamoDbContext.SaveAsync(rentGroupSummary.ToDatabase()).ConfigureAwait(false);
        }

        public async Task<List<RentGroupSummary>> GetAllRentGroupSummaryAsync(DateTime submitDate)
        {
            List<ScanCondition> scanConditions = new List<ScanCondition>();

            scanConditions.Add(new ScanCondition("SubmitDate", ScanOperator.Between, GetDayRange(submitDate).Item1, GetDayRange(submitDate).Item2));
            scanConditions.Add(new ScanCondition("TargetType", ScanOperator.In, TargetType.RentGroup));

            List<FinanceSummaryDbEntity> data = await _wrapper.ScanAsync(_dynamoDbContext, scanConditions).ConfigureAwait(false);

            return data.Select(s => s.ToRentGroupDomain()).OrderByDescending(r => r.SubmitDate).ToList();
        }

        public async Task<RentGroupSummary> GetRentGroupSummaryByNameAsync(string rentGroupName, DateTime submitDate)
        {
            List<ScanCondition> scanConditions = new List<ScanCondition>();

            scanConditions.Add(new ScanCondition("SubmitDate", ScanOperator.Between, GetDayRange(submitDate).Item1, GetDayRange(submitDate).Item2));
            scanConditions.Add(new ScanCondition("TargetType", ScanOperator.Equal, TargetType.RentGroup));
            // ToDo: Change way to search by rent_group_name
            //scanConditions.Add(new ScanCondition("RentGroupSummaryData.rent_group_name", ScanOperator.Equal, rentGroupName));

            List<FinanceSummaryDbEntity> data = await _wrapper.ScanAsync(_dynamoDbContext, scanConditions).ConfigureAwait(false);

            return data.OrderByDescending(r => r.SubmitDate).FirstOrDefault(s => string.Equals(s.RentGroupSummaryData?.RentGroupName, rentGroupName)).ToRentGroupDomain();
        }

        #endregion

        #region Get Weekly Summary
        public async Task<List<WeeklySummary>> GetAllWeeklySummaryAsync(Guid targetId, DateTime? startDate, DateTime? endDate)
        {
            var scanConditions = new List<ScanCondition>();

            if (targetId != Guid.Parse("00000000-0000-0000-0000-000000000000"))
                scanConditions.Add(new ScanCondition("TargetId", ScanOperator.Equal, targetId));
            if (startDate.HasValue && endDate.HasValue)
            {
                scanConditions.Add(new ScanCondition("WeekStartDate", ScanOperator.Between, startDate, endDate));
            }
            var transactionSummaries = await _wrapper.ScanSummaryAsync(_dynamoDbContext, scanConditions).ConfigureAwait(false);

            var result = transactionSummaries.Select(p => p.ToWeeklySummaryDomain()).ToList();

            return result.OrderByDescending(r => r.WeekStartDate).ToList();
        }

        public async Task AddAsync(WeeklySummary weeklySummary)
        {
            await _dynamoDbContext.SaveAsync(weeklySummary.ToDatabase()).ConfigureAwait(false);
        }
        public async Task<WeeklySummary> GetWeeklySummaryByIdAsync(Guid id)
        {
            var data = await _wrapper.LoadSummaryAsync(_dynamoDbContext, id).ConfigureAwait(false);

            return data?.ToWeeklySummaryDomain();
        }
        #endregion

        private static Tuple<DateTime, DateTime> GetDayRange(DateTime date)
            => new Tuple<DateTime, DateTime>(date.Date, date.Date.AddHours(23).AddMinutes(59));
    }
}
