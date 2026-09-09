using Lookify.Cep.Inteface;
using Lookify.Cep.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lookify;

public sealed class LookifyService {

    public ICepLookifyService Cep { get; }

    public LookifyService(
        IHttpClientFactory httpFactory,
        IOptions<LookifyOptions> options,
        ILogger<LookifyService> logger)
    {
        Cep = new CepLookifyService(
            httpFactory, 
            options, 
            logger);
    }
}
