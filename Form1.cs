using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Media;

namespace Genshin_Auto_Start;

public partial class Form1 : Form
{
    private PictureBox PlayBox;
    private string mciCommand = "";
    private SoundPlayer beforePlayer;
    private SoundPlayer qidongPlayer;

    private Thread? closeVideoThread;

    // 检测间隔（毫秒）
    private const int DetectionInterval = 100;
    // 检测成功后等待时间（毫秒）
    private const int PauseInterval = 15000;
    // 白色占比阈值
    private const double WhiteThreshold = 0.7;
    private bool isDetectionPaused = false;
    private System.Windows.Forms.Timer detectionTimer = new System.Windows.Forms.Timer();
    private object lockObject = new object();


    private class Libwrap
    {
        [DllImport(("winmm.dll"), EntryPoint = "mciSendString", CharSet = CharSet.Auto)]
        public static extern int mciSendString(string lpstrCommand, string? lpstrReturnString, int uReturnLength, int hwndCallback);
    }

    public Form1()
    {
        InitializeComponent();
        // 设置程序图标
        this.Icon = new Icon("logo.ico");
        // 窗体全透明
        this.BackColor = Color.Magenta;
        this.TransparencyKey = Color.Magenta;
        // 隐藏窗体顶部标题栏
        this.FormBorderStyle = FormBorderStyle.None;
        // 窗体默认全屏显示
        this.WindowState = FormWindowState.Maximized;
        // 保持窗体始终置顶
        this.TopMost = true;

        // 资源设置
        beforePlayer = new SoundPlayer("qidong_before.wav");
        qidongPlayer = new SoundPlayer("qidong.wav");
        PlayBox = this.PictureBox0;
        
        closeVideoThread = null;

        Thread.Sleep(3000);

        // 播放前奏
        PlayAudioBefore();

        // 定时“启动”检测
        detectionTimer.Interval = DetectionInterval;
        detectionTimer.Tick += DetectQidong_Tick;
        detectionTimer.Start();
    }

    private void InitVideo()
    {
        // 在PictureBox0中载入qidong.mp4视频
        mciCommand = "open qidong.mp4 alias video" + " parent " + PlayBox.Handle.ToInt32() + " style child";
        Libwrap.mciSendString(mciCommand, null, 0, 0);
        // 获取电脑屏幕宽高
        Rectangle rect = Screen.PrimaryScreen.Bounds;
        mciCommand = "put video window at 0 0 " + rect.Width + " " + rect.Height;
        Libwrap.mciSendString(mciCommand, null, 0, 0);
    }

    private void PlayVideo()
    {
        mciCommand = "play video";
        Libwrap.mciSendString(mciCommand, null, 0, 0);
    }

    private void StopVideo(int time = 0)
    {
        // 停止视频线程
        closeVideoThread = new Thread(() =>
        {
            // 等待
            Thread.Sleep(time);
            // 使用主线程关闭视频
            this.Invoke((MethodInvoker)delegate
            {
                Libwrap.mciSendString("close video", null, 0, 0);
            });
            PlayAudioBefore(500);
        });
        closeVideoThread.Start();
    }

    private void PlayAudioBefore(int time = 0)
    {
        Thread thread = new Thread(() =>
        {
            Thread.Sleep(time);
            this.Invoke((MethodInvoker)delegate
            {
                qidongPlayer.Stop();
                beforePlayer.PlayLooping();
            });
        });
        thread.Start();
    }

    private void PlayAudioQidong()
    {
        beforePlayer.Stop();
        qidongPlayer.Play();
    }

    private void PlayThenStop(int time)
    {
        this.StopVideo(time);
        this.InitVideo();
        this.PlayVideo();
        this.PlayAudioQidong();
    }

    private void DetectQidong_Tick(object? sender, EventArgs e)
    {
        if(!isDetectionPaused)
        {
            Thread detectionTread = new Thread(() =>
            {
                lock (lockObject)
                {
                    int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                    int screenHeight = Screen.PrimaryScreen.Bounds.Height;
                    int totalPixels = screenWidth * screenHeight;
                    int whitePixels = 0;

                    using (Bitmap screenCapture = new Bitmap(screenWidth, screenHeight))
                    using (Graphics graphics = Graphics.FromImage(screenCapture))
                    {
                        graphics.CopyFromScreen(0, 0, 0, 0, screenCapture.Size);
                        
                        BitmapData bmpData = screenCapture.LockBits(new Rectangle(0, 0, screenWidth, screenHeight), ImageLockMode.ReadOnly, screenCapture.PixelFormat);
                        int bytesPerPixel = Bitmap.GetPixelFormatSize(screenCapture.PixelFormat) / 8;
                        int stride = bmpData.Stride;

                        unsafe
                        {
                            byte* ptr = (byte*)bmpData.Scan0;
                            for (int y = 0; y < screenHeight; y++)
                            {
                                for (int x = 0; x < screenWidth; x++)
                                {
                                    int pixelOffset = y * stride + x * bytesPerPixel;
                                    byte b = ptr[pixelOffset];
                                    byte g = ptr[pixelOffset + 1];
                                    byte r = ptr[pixelOffset + 2];

                                    // 判断是否为白色像素
                                    if (r == 255 && g == 255 && b == 255) 
                                    {
                                        whitePixels++;
                                    }
                                }
                            }
                        }
                        screenCapture.UnlockBits(bmpData);
                    }

                    double whitePercentage = (double)whitePixels / totalPixels;
                    if (whitePercentage > WhiteThreshold)
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            detectionTimer.Stop();
                            // 启动！
                            PlayThenStop(9850);

                            isDetectionPaused = true;

                            System.Windows.Forms.Timer pauseTimer = new System.Windows.Forms.Timer();
                            pauseTimer.Interval = PauseInterval;
                            pauseTimer.Tick += (object? s, EventArgs e) =>
                            {
                                pauseTimer.Stop();
                                isDetectionPaused = false;
                                detectionTimer.Start();
                            };
                            pauseTimer.Start();
                        });
                    }
                }
            });

            detectionTread.Start();
        }
    }

    private void registers_FormClosing(object sender, FormClosingEventArgs e)
    {
        Libwrap.mciSendString("close video", null, 0, 0);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        beforePlayer.Stop();
        qidongPlayer.Stop();
        detectionTimer.Stop();
        detectionTimer.Dispose();
        PlayBox.Dispose();
        beforePlayer.Dispose();
        qidongPlayer.Dispose();

        if (closeVideoThread != null && closeVideoThread.IsAlive)
        {
            closeVideoThread.Join();
        }
    }
}