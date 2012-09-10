using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace GroupByMany
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = new List<FinDTO>();
            source.Add(new FinDTO("FUND_1", "PB_1", "USD", 1.01));
            source.Add(new FinDTO("FUND_1", "PB_1", "USD", 2.02));

            source.Add(new FinDTO("FUND_1", "PB_3", "USD", 3.03));
            source.Add(new FinDTO("FUND_1", "PB_3", "USD", 3.03));

            source.Add(new FinDTO("FUND_2", "PB_3", "USD", 3.03));
            source.Add(new FinDTO("FUND_2", "PB_3", "CAN", 3.03));

            // Print original
            Console.WriteLine("SOURCE:");
            foreach (FinDTO item in source)
                Console.WriteLine(string.Format("Fund: {0}, Prime Broker: {1}, Currency: {2}, Financing: {3}",
                                                item.Fund, item.PrimeBroker, item.Currency, item.Financing));

            // Magic										
            // var query3 = source.GroupByMany(x => x.Fund, x => x.PrimeBroker, x => x.Currency);
            var query3 = from cfd in source
                         group cfd by new { cfd.Fund, cfd.PrimeBroker, cfd.Currency } into cfdGroup
                         select new
                         {
                             cfdGroup.Key.Fund,
                             cfdGroup.Key.PrimeBroker,
                             cfdGroup.Key.Currency,
                             Financing = cfdGroup.Sum(x => x.Financing)
                         };

            // Print calculated
            Console.WriteLine("\nCALCULATED:");
            foreach (var item2 in query3)
                Console.WriteLine(string.Format("Fund: {0}, Prime Broker: {1}, Currency: {2}, Financing: {3}",
                                                item2.Fund, item2.PrimeBroker, item2.Currency, item2.Financing));

            Console.ReadLine();
        }
    }
}

public class FinDTO
{
    public string Fund;
    public string PrimeBroker;
    public string Currency;
    public double Financing;

    public FinDTO(string fund, string primeBroker, string currency, double financing)
    {
        this.Fund = fund;
        this.PrimeBroker = primeBroker;
        this.Currency = currency;
        this.Financing = financing;
    }
}


public class GroupResult
{
    public object Key { get; set; }
    public int Count { get; set; }
    public IEnumerable Items { get; set; }
    public IEnumerable<GroupResult> SubGroups { get; set; }
    public override string ToString()
    {
        return string.Format("{0} ({1})", Key, Count);
    }
}

public static class MyEnumerableExtensions
{
    public static IEnumerable<GroupResult> GroupByMany<TElement>(
        this IEnumerable<TElement> elements,
        params Func<TElement, object>[] groupSelectors)
    {
        if (groupSelectors.Length > 0)
        {
            var selector = groupSelectors.First();

            // reduce the list recursively until zero
            var nextSelectors = groupSelectors.Skip(1).ToArray();
            return
                elements.GroupBy(selector).Select(
                    g => new GroupResult
                    {
                        Key = g.Key,
                        Count = g.Count(),
                        Items = g,
                        SubGroups = g.GroupByMany(nextSelectors)
                    });
        }
        else
            return null;
    }
}