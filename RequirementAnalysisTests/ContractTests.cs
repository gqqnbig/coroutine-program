using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RequirementAnalysis.Tests
{
	public class ContractTests
	{
		[Fact]
		public void TestProcessContract()
		{
			var code = @"
	Contract ProcessSaleService::makeCashPayment(amount : Real) : Boolean {

		precondition:
			CurrentSale.oclIsUndefined() = false and CurrentSale.IsComplete = false and CurrentSale.IsReadytoPay = true and amount >= CurrentSale.Amount

		postcondition:
			let cp:CashPayment in cp.oclIsNew() and cp.AmountTendered = amount and cp.BelongedSale = CurrentSale and CurrentSale.AssoicatedPayment = cp and CurrentSale.Belongedstore = CurrentStore and CurrentStore.Sales->includes(CurrentSale) and CurrentSale.IsComplete = true and CurrentSale.Time.isEqual(Now) and cp.Balance = amount - CurrentSale.Amount and CashPayment.allInstance()->includes(cp) and result = true

	}";


			Dictionary<string, ServiceBlock> serviceDefinitions = new Dictionary<string, ServiceBlock>();
			var ProcessSaleService = new ServiceBlock();
			ProcessSaleService.Name = "ProcessSaleService";
			ProcessSaleService.Properties.Add("CurrentSale", "Sale");
			serviceDefinitions.Add("ProcessSaleService", ProcessSaleService);
			var systemService = new ServiceBlock();
			systemService.Name = "System";
			systemService.Properties.Add("CurrentStore", "Store");
			serviceDefinitions.Add("System", systemService);

			var generator = ContractAnalyzer.GetGenerator(serviceDefinitions, code, new Dictionary<string, string>());

			var count = generator.Type.Flow.Count(f => f.Direction == GeneratorCalculation.Direction.Yielding && f.Type.ToString().Equals("CurrentSale"));
			Assert.True(count == 1, "makeCashPayment should only yield CurrentSale once.");
		}
	}
}
