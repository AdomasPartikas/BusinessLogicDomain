namespace BusinessLogicDomain.Tests.Integration.Fixtures
{
    public class IntegrationTestFixture : IDisposable
    {

        public IntegrationTestFixture()
        {
            // Setup code here
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}