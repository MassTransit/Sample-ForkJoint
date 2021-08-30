namespace ForkJoint.Application.Services
{
    using Contracts;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Grill :
        IGrill
    {
        readonly ILogger<Grill> _logger;
        readonly HashSet<BurgerPatty> _patties;

        public Grill(ILogger<Grill> logger)
        {
            _logger = logger;
            _patties = new HashSet<BurgerPatty>();
        }

        public async Task<BurgerPatty> CookOrUseExistingPatty(decimal weight, bool cheese)
        {
            var existing = _patties.FirstOrDefault(x => x.Cheese == cheese && x.Weight == weight);
            if (existing != null)
            {
                _logger.LogDebug("Using existing patty {Weight} {Cheese}", existing.Weight, existing.Cheese);

                _patties.Remove(existing);
                return existing;
            }

            _logger.LogDebug("Grilling patty {Weight} {Cheese}", weight, cheese);

            await Task.Delay(5000 + (int)(1000.0m * weight));

            var patty = new BurgerPatty
            {
                Weight = weight,
                Cheese = cheese
            };

            return patty;
        }

        public void Add(BurgerPatty patty)
        {
            _patties.Add(patty);
        }
    }
}