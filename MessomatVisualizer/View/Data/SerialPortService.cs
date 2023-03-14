using System.IO.Ports;

namespace View.Data;

public class SerialPortService {
    private SerialPort mySerialPort;
    public MeasureResult SerialData { get; set; } = new();
    public string RawData = "";


    public SerialPortService() {
        mySerialPort = new SerialPort("COM3");
        mySerialPort.BaudRate = 9600;

        mySerialPort.NewLine = (@"&N");

        if (!mySerialPort.IsOpen) {
            mySerialPort.DataReceived += (DataReceivedHandler);
        }
    }

    private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e) {
        SerialPort sp = (SerialPort) sender;
        try {
            string indata = sp.ReadExisting();
            if (indata.Contains((char) 21))
                return;
            if (indata.Contains((char) 0))
                return;

            RawData += indata;
            //Console.WriteLine(RawData);
            var split = String.Empty;
            split += (char) 3;
            var dataSplit = RawData.Split(split);

            if (dataSplit.Length < 2) {
                return;
            }

            foreach (var data in (dataSplit.Where(d => d.Contains("A")))) {
                foreach (var s in data.Split(";")) {
                    if (s.Split(":")[0].Contains("LV")) {
                        Console.WriteLine(s.Split(":")[1]);
                        SerialData.Luminosity = Convert.ToInt32(s.Split(":")[1]);
                    }

                    if (s.Split(":")[0].Contains("BV")) {
                        Console.WriteLine(s.Split(":")[1]);
                        SerialData.DigitalStatus = s.Split(":")[1] == "on";
                    }
                }
            }

            foreach (var data in (dataSplit.Where(d => !d.Contains("A")))) {
                if (data.Split(":")[0].Contains("LV")) {
                    Console.WriteLine(data.Split(":")[1]);
                    SerialData.Luminosity = Convert.ToInt32(data.Split(":")[1]);
                }
            }
            
            foreach (var data in (dataSplit.Where(d => !d.Contains("A")))) {
                if (data.Split(":")[0].Contains("BV")) {
                    Console.WriteLine(data.Split(":")[1]);
                    SerialData.DigitalStatus = data.Split(":")[1] == "on";
                }
            }
        }
        
        catch (Exception ex) {
            Console.WriteLine("Error: {0}", ex.Message);
        }
    }

    public Task<MeasureResult> GetSerialValue() {
        return Task.FromResult(SerialData);
    }

    public void Write(string cmd) {
        mySerialPort.Write("<" + cmd + ">");
    }
}