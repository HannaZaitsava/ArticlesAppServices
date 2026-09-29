using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ArticlesService.Tests.IntegrationTests.Common
{
    public static class WebApplicationFactoryExtension
    {
        public static HttpClient CreateClientWithTestAuth<T>(this WebApplicationFactory<T> factory) where T : class
        {
            var client = factory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "bearer",
                "CfDJ8M6rITd4FCNGnXG4b8_2UYtn-Qd6zMfBGTH13PPo7UE3wnRzyDmNtswWCryUBwJFvpkUC3CvxPYHYjEA5hyIDVwBt_Qfu9WmNp22VmNy-tC3xexnCxScjggRSsae_iIqkztebtr5nVTHKApf1-Bh8CZnX9iW3r6rucZKuzgYukXimMExaHbDMWLOQBKRvpqASPMBe_r7N6tBDjSHfz3667wQwRmUcvuGrnp7UEO9ko6hsRTdvtnj9kouskMLaB4KN6rIlBK38qWvrnqF82GQ3Wd1vllloI5ZTV3w0cTd6dpWhdUh6l6CH7gDjO3VP3QOXV3CkEV4s57E9cQ-WmRtddPUdH0WSyhdmOojrvCqTfl6plBRBQdOXA8MaEbGXKbiNN1zhiUHpsBXo9G5DZVgLoG24_xp0AAjnkJ0clECyPeBDERyuJhazqxtnrqtw00lUZ4v3tz9VtpPGRYOGb9-Y0YmwhQjDwaysD71SK1yuKNMfwtwFTw5egr0zTmVghV5H4A6EwfPPYGXKM-LYcJsoHOgcMP8VdobhGPkIy3cTaPpCZmxBJ_dky66lHm6mLxefFgjH_qJW2ra3L_3iSngrlRkPNfm543gmUQlssAvw22UiLr5JucEJ5pbKqTyTaAbxHHLt2VN_I0XxF7lKbYru0QdRKAWPsp-vRin-dRDwy7UzK2dJD26YS3byPD2_ilIlHj7A3TXj0cgS_iGW9rf7N36WFxdYBX8Q85HS4X5Ju7xaAxJrDK7z5Lqua1-blzUPNaIGYhu24rx24RNd_VbJ-INCLQ6NduKpN1Nwwl6pbKF");
            return client;
        }
    }
}
