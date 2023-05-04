namespace ScoreBridge.Client.ViewsModels;

public class MainWindowViewModel
{
    private readonly TcpClient _tcpClient;

    public MainWindowViewModel(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;

        Name = _tcpClient.GetTest();
    }

    public string Name { get; set; }
}