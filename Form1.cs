using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

using System.Windows.Media.Imaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

// API2
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

// RestSharp
using RestSharp;

// JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ScanAPIHelper;
using SourceAFIS.Simple;

namespace ScanAPIDemo
{
    public partial class Form1 : Form
    {
        delegate void SetTextCallback(string text);

        const int FTR_ERROR_EMPTY_FRAME = 4306; /* ERROR_EMPTY */
        const int FTR_ERROR_MOVABLE_FINGER = 0x20000001;
        const int FTR_ERROR_NO_FRAME = 0x20000002;
        const int FTR_ERROR_USER_CANCELED = 0x20000003;
        const int FTR_ERROR_HARDWARE_INCOMPATIBLE = 0x20000004;
        const int FTR_ERROR_FIRMWARE_INCOMPATIBLE = 0x20000005;
        const int FTR_ERROR_INVALID_AUTHORIZATION_CODE = 0x20000006;

        /* Other return codes are Windows-compatible */
        const int ERROR_NO_MORE_ITEMS = 259;                // ERROR_NO_MORE_ITEMS
        const int ERROR_NOT_ENOUGH_MEMORY = 8;              // ERROR_NOT_ENOUGH_MEMORY
        const int ERROR_NO_SYSTEM_RESOURCES = 1450;         // ERROR_NO_SYSTEM_RESOURCES
        const int ERROR_TIMEOUT = 1460;                     // ERROR_TIMEOUT
        const int ERROR_NOT_READY = 21;                     // ERROR_NOT_READY
        const int ERROR_BAD_CONFIGURATION = 1610;           // ERROR_BAD_CONFIGURATION
        const int ERROR_INVALID_PARAMETER = 87;             // ERROR_INVALID_PARAMETER
        const int ERROR_CALL_NOT_IMPLEMENTED = 120;         // ERROR_CALL_NOT_IMPLEMENTED
        const int ERROR_NOT_SUPPORTED = 50;                 // ERROR_NOT_SUPPORTED
        const int ERROR_WRITE_PROTECT = 19;                 // ERROR_WRITE_PROTECT
        const int ERROR_MESSAGE_EXCEEDS_MAX_SIZE = 4336;    // ERROR_MESSAGE_EXCEEDS_MAX_SIZE

        private Device m_hDevice;
        private AfisEngine Afis;
        List<MyPerson> database;
        private bool m_bCancelOperation;
        private byte[] m_Frame;
        private bool m_bScanning;
        private byte m_ScanMode;
        private bool m_bExit;
        private String m_lblCompatibility = String.Empty;
        private String CaptureType;
        private String UserID;
        private String AppDir;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        private String TOKEN = String.Empty;
        byte[] blob;

        #pragma warning disable 414
        private bool m_bIsLFDSupported = false;
        #pragma warning disable 414

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [Serializable]
        class MyPerson : Person
        {
            public String Name;
        }

        /*[Serializable]
        class MyFingerprint : Fingerprint
        {
            public String Filename;
        }*/

        private MyPerson _Enroll(String filename, String name)
        {
            // Initialize empty fingerprint object and set properties
            //MyFingerprint fp = new MyFingerprint();
            //fp.Filename = filename;
            Fingerprint fp = new Fingerprint();

            // Load image from the file
            //BitmapImage image = new BitmapImage(new Uri(filename, UriKind.RelativeOrAbsolute));
            //fp.AsBitmapSource = image;
            fp.AsBitmap = new Bitmap(Bitmap.FromFile(filename));
            // Above update of fp.AsBitmapSource initialized also raw image in fp.Image
            // Check raw image dimensions, Y axis is first, X axis is second

            // Initialize empty person object and set its properties
            MyPerson person = new MyPerson();
            person.Name = name;
            // Add fingerprint to the person
            person.Fingerprints.Add(fp);

            // Execute extraction in order to initialize fp.Template
            Afis.Extract(person);
            // Check template size
            //Console.WriteLine(" Template size = {0} bytes", fp.Template.Length);

            return person;
        }

        private MyPerson Enroll(String filename, String name)
        {
            Fingerprint fp = new Fingerprint();
            fp.AsBitmap = new Bitmap(Bitmap.FromFile(filename));

            MyPerson person = new MyPerson();
            person.Name = name;
            person.Fingerprints.Add(fp);

            Afis.Extract(person);

            return person;
        }

        public Form1(String v1, String v2)
        {
            CaptureType = v1;
            UserID = v2;

            InitializeComponent();
            m_bIsLFDSupported = true;
            m_hDevice = null;
            m_ScanMode = 0;
            m_bScanning = false;
            m_bExit = false;

            Afis = new AfisEngine();
            database = new List<MyPerson>();

            AppDir = AppDomain.CurrentDomain.BaseDirectory;

            this.Text = CaptureType.ToUpper();

            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }

        private void LoadScanner()
        {
            try
            {
                int defaultInterface = ScanAPIHelper.Device.BaseInterface;
                FTRSCAN_INTERFACE_STATUS[] status = ScanAPIHelper.Device.GetInterfaces();

                foreach (var st in status)
                {
                    if (st == FTRSCAN_INTERFACE_STATUS.FTRSCAN_INTERFACE_STATUS_CONNECTED)
                    {
                        ScanAPIHelper.Device.BaseInterface = 0;
                        OpenDevice();
                        m_hDevice.DetectFakeFinger = true;
                        break;
                    }
                    m_lblCompatibility = "No Finger Device";
                }

                //RunAsyncVerify(2).Wait();
            }
            catch (ScanAPIException ex)
            {
                ShowError(ex);
            }
            m_ScanMode = 0;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            ThreadStart worker = new ThreadStart(LoadScanner);
            Thread t = new Thread(worker);
            t.IsBackground = true;
            t.Start();
        }

        private void ShowError(ScanAPIException ex)
        {
            string szMessage;
            switch( ex.ErrorCode )
            {
            case FTR_ERROR_EMPTY_FRAME:
                szMessage = "Empty Frame";
                break;

            case FTR_ERROR_MOVABLE_FINGER:
                szMessage = "Movable Finger";
                break;

            case FTR_ERROR_NO_FRAME:
                szMessage = "Fake Finger";
                break;

            case FTR_ERROR_HARDWARE_INCOMPATIBLE:
                szMessage = "Incompatible Hardware";
                break;

            case FTR_ERROR_FIRMWARE_INCOMPATIBLE:
                szMessage = "Incompatible Firmware";
                break;

            case FTR_ERROR_INVALID_AUTHORIZATION_CODE:
                szMessage = "Invalid Authorization Code";
                break;

            case ERROR_NOT_ENOUGH_MEMORY:
                szMessage = "Error code ERROR_NOT_ENOUGH_MEMORY";
                break;

            case ERROR_NO_SYSTEM_RESOURCES:
                szMessage = "Error code ERROR_NO_SYSTEM_RESOURCES";
                break;

            case ERROR_TIMEOUT:
                szMessage = "Error code ERROR_TIMEOUT";
                break;

            case ERROR_NOT_READY:
                szMessage = "Error code ERROR_NOT_READY";
                break;

            case ERROR_BAD_CONFIGURATION:
                szMessage = "Error code ERROR_BAD_CONFIGURATION";
                break;

            case ERROR_INVALID_PARAMETER:
                szMessage = "Error code ERROR_INVALID_PARAMETER";
                break;

            case ERROR_CALL_NOT_IMPLEMENTED:
                szMessage = "Error code ERROR_CALL_NOT_IMPLEMENTED";
                break;

            case ERROR_NOT_SUPPORTED:
                szMessage = "Error code ERROR_NOT_SUPPORTED";
                break;

            case ERROR_WRITE_PROTECT:
                szMessage = "Error code ERROR_WRITE_PROTECT";
                break;

            case ERROR_MESSAGE_EXCEEDS_MAX_SIZE:
                szMessage = "Error code ERROR_MESSAGE_EXCEEDS_MAX_SIZE";
                break;

            default:
                szMessage = String.Format( "Error code: {0}", ex.ErrorCode );
                break;
            }
            SetMessageText( szMessage );
        }

        private void OpenDevice()
        {
            try
            {
                m_hDevice = new Device();
                m_hDevice.Open();

                // gets devce parameters
                VersionInfo version = m_hDevice.VersionInformation;
                //m_lblApiVersion.Text = version.APIVersion.ToString();
                //m_lblHardwareVersion.Text = version.HardwareVersion.ToString();
                //m_lblFirmwareVersion.Text = version.FirmwareVersion.ToString();

                m_bIsLFDSupported = false;
                DeviceInfo dinfo = m_hDevice.Information;
                switch (dinfo.DeviceCompatibility)
                {
                    case 0:
                    case 1:
                    case 4:
                    case 11:
                        m_lblCompatibility = "FS80";
                        m_bIsLFDSupported = true;
                        break;
                    case 5:
                        m_lblCompatibility = "FS88";
                        m_bIsLFDSupported = true;
                        break;
                    case 7:
                        m_lblCompatibility = "FS50";
                        break;
                    case 8:
                        m_lblCompatibility = "FS60";
                        break;
                    case 9:
                        m_lblCompatibility = "FS25";
                        m_bIsLFDSupported = true;
                        break;
                    case 10:
                        m_lblCompatibility = "FS10";
                        break;
                    case 13:
                        m_lblCompatibility = "FS80H";
                        m_bIsLFDSupported = true;
                        break;
                    case 14:
                        m_lblCompatibility = "FS88H";
                        m_bIsLFDSupported = true;
                        break;
                    case 15:
                        m_lblCompatibility = "FS64";
                        break;
                    case 16:
                        m_lblCompatibility = "FS26E";
                        break;
                    case 17:
                        m_lblCompatibility = "FS80HS";
                        break;
                    case 18:
                        m_lblCompatibility = "FS26";
                        break;
                    default:
                        m_lblCompatibility = "Unknown device";
                        break;
                }

                m_Dev_Name.Text = m_lblCompatibility + " / " + version.HardwareVersion.ToString();
                m_hDevice.InvertImage = false;

                m_ScanMode = 4;

                Capature();
            }
            catch (ScanAPIException ex)
            {
                if (m_hDevice != null)
                {
                    m_hDevice.Dispose();
                    m_hDevice = null;
                }
                ShowError(ex);
            }
        }

        private void CloseDevice()
        {
            m_bCancelOperation = true;
            Size size = m_hDevice.ImageSize;
            m_hDevice.Dispose();
            m_hDevice = null;

            //m_lblApiVersion.Text = String.Empty;
            //m_lblHardwareVersion.Text = String.Empty;
            //m_lblFirmwareVersion.Text = String.Empty;

            //m_lblCompatibility.Text = String.Empty;

            m_picture.Image = null;

            m_btnSave.Enabled = false;
        }

        private void SetMessageText(string text)
        {
            if (m_bExit)
                return;

            if (this.m_textMessage.InvokeRequired)
            {
                try
                {
                    SetTextCallback d = new SetTextCallback(this.SetMessageText);
                    this.Invoke(d, new object[] { text });
                }
                catch(Exception)
                {
                    this.Update();
                    throw;
                }
            }
            else
            {
                this.m_textMessage.Text = text;
                this.Update();
            }
        }

        private void GetFrame()
        {
            try
            {
                if (m_ScanMode == 0)
                    m_Frame = m_hDevice.GetFrame();
                else
                    m_Frame = m_hDevice.GetImage(m_ScanMode);

                SetMessageText("Finger OK");

                MyBitmapFile myFile = new MyBitmapFile(m_hDevice.ImageSize.Width, m_hDevice.ImageSize.Height, m_Frame);

                System.Threading.Thread.Sleep(50);

                CloseFile(AppDir + @"\candidate.bmp");
                //CloseFile("database.dat");

                if (CaptureType.ToLower() == "enrollment")
                {
                    try
                    {
                        Stream file = File.Create(AppDir + @"\candidate.bmp");
                        file.Write(myFile.BitmatFileData, 0, myFile.BitmatFileData.Length);
                        file.Close();

                        blob = imageToByteArray(Image.FromFile(AppDir + @"\candidate.bmp"));
                        
                        MessageBox.Show(System.Text.Encoding.UTF8.GetString(blob));

                        database.Clear();
                        database.Add(Enroll(AppDir + @"\candidate.bmp", UserID));

                        BinaryFormatter formatter = new BinaryFormatter();

                        Stream stream = File.Create(AppDir + @"\database.dat");
                        formatter.Serialize(stream, database);
                        stream.Close();

                        RunAsyncEnroll().Wait();

                        SetMessageText("Enrollment Done");
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }

                if(CaptureType.ToLower() == "verification")
                {
                    Stream file = File.Create(AppDir + @"\candidate.bmp");
                    //Stream fs = File.Create(AppDir + @"\database.dat");
                    try
                    {
                        file.Write(myFile.BitmatFileData, 0, myFile.BitmatFileData.Length);
                        file.Close();

                        database.Clear();

                        RunAsyncVerify(int.Parse(UserID)).Wait();

                        //BinaryFormatter formatter = new BinaryFormatter();
                        //database = (List<MyPerson>)formatter.Deserialize(fs);

                        Fingerprint fp2 = new Fingerprint();
                        fp2.AsBitmap = new Bitmap(byteArrayToImage(blob));

                        MyPerson person2 = new MyPerson();
                        person2.Name = UserID;
                        person2.Fingerprints.Add(fp2);
                        Afis.Extract(person2);

                        Fingerprint fp1 = new Fingerprint();
                        fp1.AsBitmap = new Bitmap(Bitmap.FromFile(AppDir + @"\candidate.bmp"));

                        MyPerson person1 = new MyPerson();
                        person1.Name = UserID;
                        person1.Fingerprints.Add(fp1);
                        Afis.Extract(person1);

                        float Score = Afis.Verify(person2, person1);
                        if (Score > 0)
                        {
                            SetMessageText(Score.ToString());
                        }
                    }
                    catch (Exception er)
                    {
                        //throw;
                        MessageBox.Show(er.InnerException.Message);
                    }
                    finally
                    {
                        //fs.Close();
                    }
                }

                
                this.Close();
            }
            catch (ScanAPIException ex)
            {
                if (m_Frame != null)
                    m_Frame = null;
                ShowError(ex);
            }
        }

        private async Task RunAsyncVerify(int id)
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();

                var client = new RestClient("http://localhost:3293/api/token");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddHeader("postman-token", "7b69394f-bd17-e5be-ea62-fa46a435be6f");
                request.AddHeader("cache-control", "no-cache");
                request.AddParameter("application/x-www-form-urlencoded", "grant_type=password&username=esarearthur&password=123456", ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

                TOKEN = JsonGetKey(FormatJson(response.Content), "access_token");

                var FP_Details = new FingerPrintDetails()
                {
                    // Required value so place a dummy value here -1
                    FP_ID = -1,
                    // Required value so place a dummy value here DUMMY
                    FP_NAME = "DUMMY",
                    // Leave this as it is
                    FP_BLOB01 = blob
                };

                client = new RestClient("http://localhost:3293/api/verify/" + id.ToString());
                request = new RestRequest(Method.POST);
                request.AddHeader("postman-token", "ac68cd3a-db3a-5340-438f-f190ddd53bde");
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("authorization", "bearer " + TOKEN);
                request.AddParameter("application/json", JsonConvert.SerializeObject(FP_Details, Formatting.Indented), ParameterType.RequestBody);
                response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

                MessageBox.Show(response.Content);
                //MessageBox.Show(JsonGetKey(FormatJson(response.Content), "FP_BLOB01"));
                blob = Encoding.ASCII.GetBytes(JsonGetKey(FormatJson(response.Content), "FP_BLOB01"));
            }
            catch(Exception er)
            {
                MessageBox.Show(er.Message);
            }
        }

        private async Task RunAsyncEnroll()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var client = new RestClient("http://localhost:3293/api/token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("postman-token", "7b69394f-bd17-e5be-ea62-fa46a435be6f");
            request.AddHeader("cache-control", "no-cache");
            request.AddParameter("application/x-www-form-urlencoded", "grant_type=password&username=esarearthur&password=123456", ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            TOKEN = JsonGetKey(FormatJson(response.Content), "access_token");

            var FP_Details = new FingerPrintDetails()
            {
                FP_ID = int.Parse(UserID),
                FP_NAME = "ANOTHER JOHN DOE",
                FP_BLOB01 = blob
            };

            client = new RestClient("http://localhost:3293/api/FingerPrintDetails");
            request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("postman-token", "239d676d-3164-1906-7f58-7afbdd31f0b6");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "bearer " + TOKEN);
            request.AddParameter("application/json", JsonConvert.SerializeObject(FP_Details, Formatting.Indented), ParameterType.RequestBody);
            response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            MessageBox.Show(response.Content);
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        private String FormatJson(String json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        private String JsonGetKey(String json, String key)
        {
            JObject Json_Key = JObject.Parse(json);
            return (String)Json_Key[key];
        }

        private void Capature()
        {
            if (!m_bScanning)
            {
                m_bCancelOperation = false;
                m_btnSave.Enabled = false;
                Thread WorkerThread = new Thread(new ThreadStart(CaptureThread));
                WorkerThread.Start();
            }
            else
            {
                m_bCancelOperation = true;
                if (m_Frame != null)
                    m_btnSave.Enabled = true;
            }
        }

        private void CaptureThread()
        {
            m_bScanning = true;
            while (!m_bCancelOperation)
            {
                GetFrame();
                if (m_Frame != null)
                {
                    MyBitmapFile myFile = new MyBitmapFile(m_hDevice.ImageSize.Width, m_hDevice.ImageSize.Height, m_Frame);
                    MemoryStream BmpStream = new MemoryStream(myFile.BitmatFileData);
                    Bitmap Bmp = new Bitmap(BmpStream);
                    m_picture.Image = Bmp;
                }
                else
                    m_picture.Image = null;
                Thread.Sleep(10);
            }
            m_bScanning = false;
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            m_bExit = true;
            m_bCancelOperation = true;
            if (m_hDevice != null)
            {
                m_hDevice.Dispose();
                m_hDevice = null;
            }
        }

        private void CloseFile(String _FileName)
        {
            if (File.Exists(_FileName))
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                FileInfo f = new FileInfo(_FileName);
                f.Delete();
            }
        }

        private void m_btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlgSave = new SaveFileDialog();
            dlgSave.Filter = "bmp files (*.bmp)|*.bmp|wsq files (*.wsq)|*.wsq";
            if (dlgSave.ShowDialog() == DialogResult.OK)
            {
                if (dlgSave.FilterIndex == 1)
                {
                    MyBitmapFile myFile = new MyBitmapFile(m_hDevice.ImageSize.Width, m_hDevice.ImageSize.Height, m_Frame);
                    FileStream file = new FileStream(dlgSave.FileName, FileMode.Create);
                    file.Write(myFile.BitmatFileData, 0, myFile.BitmatFileData.Length);
                    file.Close();
                    SetMessageText("Bitmap file is saved to " + dlgSave.FileName);
                }
                else //wsq
                {
                    float fBitRate = 0.75f; // in the range of 0.75 - 2.25, lower value with higher compression rate
                    byte[] wsqImage = m_hDevice.WSQ_FromRawImage(m_Frame, m_hDevice.ImageSize.Width, m_hDevice.ImageSize.Height, fBitRate);
                    if (wsqImage != null)
                    {
                        FileStream file = new FileStream(dlgSave.FileName, FileMode.Create);
                        file.Write(wsqImage, 0, wsqImage.Length);
                        file.Close();
                        SetMessageText("WSQ file is saved to " + dlgSave.FileName);
                    }                        
                }
            }
        }
    }
}
