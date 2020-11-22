using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.ComponentModel;
using System.Security.Permissions;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Windows.Threading;
using System.Windows.Markup;
using System.Threading;
using WMPLib;

namespace TTG
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TTG_setting : Window
    {

        // メディアプレーヤークラスのインスタンスを作成する
        WindowsMediaPlayer _mediaPlayer = new WindowsMediaPlayer();

        //タイマー
        private DispatcherTimer timer;

        TTG_viewer vw ;

        public TTG_setting()
        {
            InitializeComponent();

            timer = CreateTimer();
            this.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);
            this.DataContext = new MainWindowViewModel();
        }

        //フォント選択枠
        class MainWindowViewModel
        {
            public IEnumerable<FontFamily> FontList { get; set; }

            public MainWindowViewModel()
            {
                this.FontList = Fonts.SystemFontFamilies;
            }
        }


        ////メッセージ動作時のフラグ類////////////////////////////

        public int[] Message_f = new int[3]; //1～3の各メッセージが有効かのフラグ　True 又は False

        public int[] Message_c = new int[3]; //1～3の各メッセージの累計表示回数カウンター

        public string[] Message_s = new string[3];//1～3の各メッセージの表示メッセージ格納


        // テロップの動作モードフラグ///////////////////////////////////
        // 0=開始してない
        // 1=タイマー待機
        // 2=テロップスライドイン
        // 3=メッセージ表示
        // 4=テロップスライドアウト

        public bool Test_f = false;
        public int T_mode = -1;
        public int SlideInType = 0;
        public int SlideOutType = 0;

        public double winSize = 500;



        private DispatcherTimer CreateTimer()
        {
            // タイマー生成（優先度はアイドル時に設定）
            var t = new DispatcherTimer(DispatcherPriority.SystemIdle);

            // タイマーイベントの発生間隔を設定
            t.Interval = TimeSpan.FromMilliseconds(20);

            // タイマーイベントの定義
            t.Tick += (sender, e) =>
            {


                // 現在の時分秒をテキストに設定
                string NowTimer = DateTime.Now.ToString("HH:mm:ss");
                NowTime.Text = NowTimer;

                // タイマーイベント発生時の処理をここに書く
                //テロップ動作時のメインの処理


                //タイマー動作中のタイマー判定
                if (T_mode > 0 && Test_f == false)
                {
                    string NowTime = NowTimer.Substring(0, 5);

                    if (NextTime1.Text == NowTime && Message_f[0] == 0)
                    {
                        Message_f[0] = Message_f.Max()+1;
                        
                        Message_c[0]++;

                        Message_s[0]=TimeToMessage(Message1.Text,1);
                        NextTime1.Text = Time_plus_m(NextTime1.Text, Timer1.Text);
                        if(T_mode<2) { T_mode = 2;  }
                    }

                    if (NextTime2.Text == NowTime && Message_f[1] == 0)
                    {
                        Message_f[1] = Message_f.Max() + 1;

                        Message_c[1]++;

                        Message_s[1]= TimeToMessage(Message2.Text, 2);
                        NextTime2.Text = Time_plus_m(NextTime2.Text, Timer2.Text);
                        if (T_mode < 2) { T_mode = 2;  }
                    }

                    if (NextTime3.Text == NowTime && Message_f[2] == 0)
                    {
                        Message_f[2] = Message_f.Max() + 1;

                        Message_c[2]++;

                        Message_s[2]= TimeToMessage(Message3.Text, 3);
                        NextTime3.Text = Time_plus_m(NextTime3.Text, Timer3.Text);
                        if (T_mode < 2) { T_mode = 2;  }
                    }
                }

                //スライドインモード
                if (T_mode == 2)
                {
                    int speed = Convert.ToInt32(InSpeed.Text);

                    //SlideType = 0 ←右から左
                    if (SlideInType == 0)
                    {
                        //メッセージテロップ位置初期化
                        Double i1_ML = vw.Image1.Margin.Left - speed;
                        Double i2_ML = vw.Image2.Margin.Left - speed;
                        Double i3_ML = vw.Image3.Margin.Left - speed;
                        Double MG_ML = vw.MessageGrid.Margin.Left - speed;

                        //目標座標の取得
                        Double I1_x = Convert.ToDouble(Image1_x.Text);
                        Double I2_x = Convert.ToDouble(Image2_x.Text);
                        Double I3_x = Convert.ToDouble(Image3_x.Text);
                        Double MG_x = Convert.ToDouble(Message_x.Text);

                        //目標座標を過ぎないように目標座標で頭打ち
                        if (i1_ML < I1_x) { i1_ML = I1_x; }
                        if (i2_ML < I2_x) { i2_ML = I2_x; }
                        if (i3_ML < I3_x) { i3_ML = I3_x; }
                        if (MG_ML < MG_x) { MG_ML = MG_x; }

                        //座標を反映
                        vw.Image1.Margin = new Thickness(i1_ML, vw.Image1.Margin.Top, 0, 0);
                        vw.Image2.Margin = new Thickness(i2_ML, vw.Image2.Margin.Top, 0, 0);
                        vw.Image3.Margin = new Thickness(i3_ML, vw.Image3.Margin.Top, 0, 0);
                        vw.MessageGrid.Margin = new Thickness(MG_ML, vw.MessageGrid.Margin.Top, 0, 0);

                        //スライドインが終わったら次のモードへ
                        if (i1_ML == I1_x && i2_ML == I2_x && i3_ML == I3_x && MG_ML == MG_x) { T_mode = 3; }
                    }
                    //SlideType = 1 →左から右
                    else if (SlideInType == 1)
                    {
                        //メッセージテロップ位置初期化
                        Double i1_ML = vw.Image1.Margin.Left + speed;
                        Double i2_ML = vw.Image2.Margin.Left + speed;
                        Double i3_ML = vw.Image3.Margin.Left + speed;
                        Double MG_ML = vw.MessageGrid.Margin.Left + speed;

                        //目標座標の取得
                        Double I1_x = Convert.ToDouble(Image1_x.Text);
                        Double I2_x = Convert.ToDouble(Image2_x.Text);
                        Double I3_x = Convert.ToDouble(Image3_x.Text);
                        Double MG_x = Convert.ToDouble(Message_x.Text);

                        //目標座標を過ぎないように目標座標で頭打ち
                        if (i1_ML > I1_x) { i1_ML = I1_x; }
                        if (i2_ML > I2_x) { i2_ML = I2_x; }
                        if (i3_ML > I3_x) { i3_ML = I3_x; }
                        if (MG_ML > MG_x) { MG_ML = MG_x; }

                        //座標を反映
                        vw.Image1.Margin = new Thickness(i1_ML, vw.Image1.Margin.Top, 0, 0);
                        vw.Image2.Margin = new Thickness(i2_ML, vw.Image2.Margin.Top, 0, 0);
                        vw.Image3.Margin = new Thickness(i3_ML, vw.Image3.Margin.Top, 0, 0);
                        vw.MessageGrid.Margin = new Thickness(MG_ML, vw.MessageGrid.Margin.Top, 0, 0);

                        //スライドインが終わったら次のモードへ
                        if (i1_ML == I1_x && i2_ML == I2_x && i3_ML == I3_x && MG_ML == MG_x) { T_mode = 3; }
                    }
                    //SlideType = 2 ↑下から上
                    else if (SlideInType == 2)
                    {
                        //メッセージテロップ位置初期化
                        Double i1_MT = vw.Image1.Margin.Top - speed;
                        Double i2_MT = vw.Image2.Margin.Top - speed;
                        Double i3_MT = vw.Image3.Margin.Top - speed;
                        Double MG_MT = vw.MessageGrid.Margin.Top - speed;

                        //目標座標の取得
                        Double I1_y = Convert.ToDouble(Image1_y.Text);
                        Double I2_y = Convert.ToDouble(Image2_y.Text);
                        Double I3_y = Convert.ToDouble(Image3_y.Text);
                        Double MG_y = Convert.ToDouble(Message_y.Text);

                        //目標座標を過ぎないように目標座標で頭打ち
                        if (i1_MT < I1_y) { i1_MT = I1_y; }
                        if (i2_MT < I2_y) { i2_MT = I2_y; }
                        if (i3_MT < I3_y) { i3_MT = I3_y; }
                        if (MG_MT < MG_y) { MG_MT = MG_y; }
                        
                        //座標を反映
                        vw.Image1.Margin = new Thickness(vw.Image1.Margin.Left, i1_MT, 0, 0);
                        vw.Image2.Margin = new Thickness(vw.Image2.Margin.Left, i2_MT, 0, 0);
                        vw.Image3.Margin = new Thickness(vw.Image3.Margin.Left, i3_MT, 0, 0);
                        vw.MessageGrid.Margin = new Thickness(vw.MessageGrid.Margin.Left, MG_MT, 0, 0);

                        //スライドインが終わったら次のモードへ
                        if (i1_MT == I1_y && i2_MT == I2_y && i3_MT == I3_y && MG_MT == MG_y) { T_mode = 3; }

                    }
                    //SlideType = 3 ↓上から下
                    else if (SlideInType == 3)
                    {
                        //メッセージテロップ位置初期化
                        Double i1_MT = vw.Image1.Margin.Top + speed;
                        Double i2_MT = vw.Image2.Margin.Top + speed;
                        Double i3_MT = vw.Image3.Margin.Top + speed;
                        Double MG_MT = vw.MessageGrid.Margin.Top + speed;

                        //目標座標の取得
                        Double I1_y = Convert.ToDouble(Image1_y.Text);
                        Double I2_y = Convert.ToDouble(Image2_y.Text);
                        Double I3_y = Convert.ToDouble(Image3_y.Text);
                        Double MG_y = Convert.ToDouble(Message_y.Text);

                        //目標座標を過ぎないように目標座標で頭打ち
                        if (i1_MT > I1_y) { i1_MT = I1_y; }
                        if (i2_MT > I2_y) { i2_MT = I2_y; }
                        if (i3_MT > I3_y) { i3_MT = I3_y; }
                        if (MG_MT > MG_y) { MG_MT = MG_y; }

                        //座標を反映
                        vw.Image1.Margin = new Thickness(vw.Image1.Margin.Left, i1_MT, 0, 0);
                        vw.Image2.Margin = new Thickness(vw.Image2.Margin.Left, i2_MT, 0, 0);
                        vw.Image3.Margin = new Thickness(vw.Image3.Margin.Left, i3_MT, 0, 0);
                        vw.MessageGrid.Margin = new Thickness(vw.MessageGrid.Margin.Left, MG_MT, 0, 0);

                        //スライドインが終わったら次のモードへ
                        if (i1_MT == I1_y && i2_MT == I2_y && i3_MT == I3_y && MG_MT == MG_y) { T_mode = 3; }
                    }

                    if (T_mode == 3 && SE1Active.IsChecked == true)
                    {
                        Sound_play(SE1_path.Text);
                    }

                }

                //メッセージ表示モード
                else if (T_mode == 3)
                {

                    int Active_m = Array.IndexOf( Message_f, 1 );
                    if (Active_m != -1)
                    {
                        //MessageBox.Show(Convert.ToString(Active_m));
                        vw.message.Text = Message_s[Active_m];

                        int Ms = Convert.ToInt32(SlideSpeed.Text);

                        Double M_ML = Canvas.GetLeft(vw.message) - Ms;
                        Double M_y = -1 * (vw.message.ActualWidth + vw.MessageGrid.Margin.Left);

                        //メッセージのスクロールが終わった時の処理
                        if (M_ML < M_y)
                        {

                            //表示が終わったメッセージのフラグを戻す
                            //ただし、タイマーのタイプが「時刻指定」の場合、
                            //一度のみの動作のため「-1」にすることで連続動作を回避する。
                            if (Active_m == 0 && MessageType1.SelectedIndex == 0) { Message_f[Active_m] = -1; }
                            else if (Active_m == 1 && MessageType2.SelectedIndex == 0) { Message_f[Active_m] = -1; }
                            else if (Active_m == 2 && MessageType3.SelectedIndex == 0) { Message_f[Active_m] = -1; }
                            else { Message_f[Active_m] = 0; }


                            //メッセージのフラグの最小値を1にして、他のフラグもそれの連番に修正する。
                            int c = 0, f_Min = 10;

                            //Message_fの中で0以外の最小値を探る
                            for (c = 0; c < Message_f.Length; c++)
                            {
                                if (Message_f[c] != 0 && Message_f[c] > 1 && Message_f[c] < f_Min) { f_Min = Message_f[c]; }
                            }

                            //Message_fで最小値が1より大きい場合に1にし、それ以外の大きい数があれば連番にしていく
                            //例：[0,4,5]→[0,1,2]
                            for (c = 0; c < Message_f.Length; c++)
                            {
                                if (Message_f[c] > 0)
                                {
                                    Message_f[c] -= f_Min - 1;
                                }
                            }

                            M_ML = Convert.ToDouble(Message_w.Text);
                        }

                        //座標を反映
                        Canvas.SetLeft(vw.message, Convert.ToDouble(M_ML));
                    }

                    else
                    {
                        T_mode = 4;
                        if (SE2Active.IsChecked == true)
                        {
                            Sound_play(SE2_path.Text);
                        }
                    }

                }

                //スライドアウトモード
                else if (T_mode == 4)
                {
                    int speed = Convert.ToInt32(OutSpeed.Text);

                    //スライド方向補正変数
                    Double slide_x = 0, slide_y = 0;

                    //スライド方向による初期位置補正変数
                    if (SlideOutType == 0)
                    {
                        slide_x = -1 * Convert.ToDouble(BGC_W.Text);
                        slide_y = 0;

                    }
                    else if (SlideOutType == 1)
                    {
                        slide_x = Convert.ToDouble(BGC_W.Text);
                        slide_y = 0;
                    }
                    else if (SlideOutType == 2)
                    {
                        slide_x = 0;
                        slide_y = -1 * Convert.ToDouble(BGC_H.Text);
                    }
                    else if (SlideOutType == 3)
                    {
                        slide_x = 0;
                        slide_y = Convert.ToDouble(BGC_H.Text);
                    }

                    //SlideType = 1 ←右から左
                    if (SlideOutType == 1)
                    {
                        //メッセージテロップ位置初期化
                        Double i1_ML = vw.Image1.Margin.Left + speed;
                        Double i2_ML = vw.Image2.Margin.Left + speed;
                        Double i3_ML = vw.Image3.Margin.Left + speed;
                        Double MG_ML = vw.MessageGrid.Margin.Left + speed;

                        //目標座標の取得
                        Double I1_x = Convert.ToDouble(Image1_x.Text) + slide_x;
                        Double I2_x = Convert.ToDouble(Image2_x.Text) + slide_x;
                        Double I3_x = Convert.ToDouble(Image3_x.Text) + slide_x;
                        Double MG_x = Convert.ToDouble(Message_x.Text) + slide_x;

                        //目標座標を過ぎないように目標座標で頭打ち
                        if (i1_ML > I1_x) { i1_ML = I1_x; }
                        if (i2_ML > I2_x) { i2_ML = I2_x; }
                        if (i3_ML > I3_x) { i3_ML = I3_x; }
                        if (MG_ML > MG_x) { MG_ML = MG_x; }

                        //座標を反映
                        vw.Image1.Margin = new Thickness(i1_ML, vw.Image1.Margin.Top, 0, 0);
                        vw.Image2.Margin = new Thickness(i2_ML, vw.Image2.Margin.Top, 0, 0);
                        vw.Image3.Margin = new Thickness(i3_ML, vw.Image3.Margin.Top, 0, 0);
                        vw.MessageGrid.Margin = new Thickness(MG_ML, vw.MessageGrid.Margin.Top, 0, 0);

                        //スライドアウトが終わったら元のモードへ。T_mode=1
                        if (i1_ML == I1_x && i2_ML == I2_x && i3_ML == I3_x && MG_ML == MG_x) { T_mode = 1;}
                    }

                    //SlideType = 0 →左から右
                    if (SlideOutType == 0)
                    {
                        //メッセージテロップ位置初期化
                        Double i1_ML = vw.Image1.Margin.Left - speed;
                        Double i2_ML = vw.Image2.Margin.Left - speed;
                        Double i3_ML = vw.Image3.Margin.Left - speed;
                        Double MG_ML = vw.MessageGrid.Margin.Left - speed;

                        //目標座標の取得
                        Double I1_x = Convert.ToDouble(Image1_x.Text) + slide_x;
                        Double I2_x = Convert.ToDouble(Image2_x.Text) + slide_x;
                        Double I3_x = Convert.ToDouble(Image3_x.Text) + slide_x;
                        Double MG_x = Convert.ToDouble(Message_x.Text) + slide_x;

                        //目標座標を過ぎないように目標座標で頭打ち
                        if (i1_ML < I1_x) { i1_ML = I1_x; }
                        if (i2_ML < I2_x) { i2_ML = I2_x; }
                        if (i3_ML < I3_x) { i3_ML = I3_x; }
                        if (MG_ML < MG_x) { MG_ML = MG_x; }

                        //座標を反映
                        vw.Image1.Margin = new Thickness(i1_ML, vw.Image1.Margin.Top, 0, 0);
                        vw.Image2.Margin = new Thickness(i2_ML, vw.Image2.Margin.Top, 0, 0);
                        vw.Image3.Margin = new Thickness(i3_ML, vw.Image3.Margin.Top, 0, 0);
                        vw.MessageGrid.Margin = new Thickness(MG_ML, vw.MessageGrid.Margin.Top, 0, 0);

                        //スライドアウトが終わったら元のモードへ。T_mode=1
                        if (i1_ML == I1_x && i2_ML == I2_x && i3_ML == I3_x && MG_ML == MG_x) { T_mode = 1;}
                    }

                    //SlideType = 3 ↑下から上
                    else if (SlideOutType == 3)
                    {
                        //メッセージテロップ位置初期化
                        Double i1_MT = vw.Image1.Margin.Top + speed;
                        Double i2_MT = vw.Image2.Margin.Top + speed;
                        Double i3_MT = vw.Image3.Margin.Top + speed;
                        Double MG_MT = vw.MessageGrid.Margin.Top + speed;

                        //目標座標の取得
                        Double I1_y = Convert.ToDouble(Image1_y.Text) + slide_y;
                        Double I2_y = Convert.ToDouble(Image2_y.Text) + slide_y;
                        Double I3_y = Convert.ToDouble(Image3_y.Text) + slide_y;
                        Double MG_y = Convert.ToDouble(Message_y.Text) + slide_y;

                        //目標座標を過ぎないように目標座標で頭打ち
                        if (i1_MT > I1_y) { i1_MT = I1_y; }
                        if (i2_MT > I2_y) { i2_MT = I2_y; }
                        if (i3_MT > I3_y) { i3_MT = I3_y; }
                        if (MG_MT > MG_y) { MG_MT = MG_y; }

                        //座標を反映
                        vw.Image1.Margin = new Thickness(vw.Image1.Margin.Left, i1_MT, 0, 0);
                        vw.Image2.Margin = new Thickness(vw.Image2.Margin.Left, i2_MT, 0, 0);
                        vw.Image3.Margin = new Thickness(vw.Image3.Margin.Left, i3_MT, 0, 0);
                        vw.MessageGrid.Margin = new Thickness(vw.MessageGrid.Margin.Left, MG_MT, 0, 0);

                        //スライドインが終わったら次のモードへ
                        if (i1_MT == I1_y && i2_MT == I2_y && i3_MT == I3_y && MG_MT == MG_y) { T_mode = 1;}

                    }

                    //SlideType = 2 ↓上から下
                    else if (SlideOutType == 2)
                    {
                        //メッセージテロップ位置初期化
                        Double i1_MT = vw.Image1.Margin.Top - speed;
                        Double i2_MT = vw.Image2.Margin.Top - speed;
                        Double i3_MT = vw.Image3.Margin.Top - speed;
                        Double MG_MT = vw.MessageGrid.Margin.Top - speed;

                        //目標座標の取得
                        Double I1_y = Convert.ToDouble(Image1_y.Text) + slide_y;
                        Double I2_y = Convert.ToDouble(Image2_y.Text) + slide_y;
                        Double I3_y = Convert.ToDouble(Image3_y.Text) + slide_y;
                        Double MG_y = Convert.ToDouble(Message_y.Text) + slide_y;

                        //目標座標を過ぎないように目標座標で頭打ち
                        if (i1_MT < I1_y) { i1_MT = I1_y; }
                        if (i2_MT < I2_y) { i2_MT = I2_y; }
                        if (i3_MT < I3_y) { i3_MT = I3_y; }
                        if (MG_MT < MG_y) { MG_MT = MG_y; }

                        //座標を反映
                        vw.Image1.Margin = new Thickness(vw.Image1.Margin.Left, i1_MT, 0, 0);
                        vw.Image2.Margin = new Thickness(vw.Image2.Margin.Left, i2_MT, 0, 0);
                        vw.Image3.Margin = new Thickness(vw.Image3.Margin.Left, i3_MT, 0, 0);
                        vw.MessageGrid.Margin = new Thickness(vw.MessageGrid.Margin.Left, MG_MT, 0, 0);

                        //スライドインが終わったら次のモードへ
                        if (i1_MT == I1_y && i2_MT == I2_y && i3_MT == I3_y && MG_MT == MG_y) { T_mode = 1;}

                    }

                    if (T_mode == 1)
                    {
                        SlideInSetting();

                        if (Test_f == true)
                        {
                            //テストボタンを「開始」表記に変える
                            TestButton.Content = "テスト開始";
                            TestButton.Foreground = new SolidColorBrush(Colors.Black);

                            //停止時の処理
                            Trp_set_off();
                            StartButton.IsEnabled = true;

                            //タイマーモードを戻す
                            T_mode = 0;
                            Test_f = false;
                        }
                    }
                    
                }

                else if(T_mode == -1)
                {
                    ParametersUpdate();
                    MessageActive1.IsEnabled = true;
                    T_mode = 0 ;
                }


            };

            // 生成したタイマーを返す
            return t;
        }


        //TTG_setting_windowのロード時の処理
        private void TTG_setting_window_Loaded(object sender, RoutedEventArgs e)
        {
            
            vw = new TTG_viewer();
            vw.Owner = this;
            vw.Show();
            
            //時刻表示開始
            timer.Start();

            //設定呼び出し
            SettingsLoad(0);
            ActiveButtonCheck();
        }


        //TTG_setting_windowを閉じる時の処理
        private void TTG_setting_window_Closing(object sender, CancelEventArgs e)
        {
            SettingsSave(0);
        }


        //背景色スライダーの値からカラーコードを生成
        private void BGC_changed_16(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BGC.Text = "#" + String.Format("{0:x6}", (Convert.ToInt32(R_slider.Value) * 65536) +
                                                (Convert.ToInt32(G_slider.Value) * 256) +
                                                 Convert.ToInt32(B_slider.Value));
            //viewerに反映
            vw.Background = BGC_sample.Background;
        }

        //背景色のカラーコード入力が正しいか判断し、正しければ他へ反映
        private void BGC_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Encoding.GetEncoding("Shift_JIS").GetByteCount(BGC.Text) == 7)
            {
                string BGC_16 = BGC.Text.Substring(1, 6);
                if (BGC.Text.Substring(0, 1) == "#" && IsHexString(BGC_16) == true && vw != null)
                {
                    R_slider.Value = Int32.Parse(BGC_16.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    G_slider.Value = Int32.Parse(BGC_16.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    B_slider.Value = Int32.Parse(BGC_16.Substring(4), System.Globalization.NumberStyles.HexNumber);
                }

            }
        }

        //文字列スライダーの値からカラーコードを生成
        private void TextC_changed_16(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TextC.Text = "#" + String.Format("{0:x6}", (Convert.ToInt32(R_slider_T.Value) * 65536) +
                                                (Convert.ToInt32(G_slider_T.Value) * 256) +
                                                 Convert.ToInt32(B_slider_T.Value));
            //viewerに反映
            //ビューウィンドウの呼び出し
            //TTG_viewer vw = Owner as TTG_viewer;
            vw.message.Foreground = TextC_sample.Background;
        }

        //テキストのカラーコード入力が正しいか判断し、正しければ他へ反映

        private void TextC_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Encoding.GetEncoding("Shift_JIS").GetByteCount(TextC.Text) == 7)
            {
                string TextC_16 = TextC.Text.Substring(1, 6);
                if (TextC.Text.Substring(0, 1) == "#" && IsHexString(TextC_16) == true && vw != null)
                {
                    R_slider_T.Value = Int32.Parse(TextC_16.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    G_slider_T.Value = Int32.Parse(TextC_16.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    B_slider_T.Value = Int32.Parse(TextC_16.Substring(4), System.Globalization.NumberStyles.HexNumber);
                }

            }
        }

        // 文字列が16進文字列かどうか判定します。
        public bool IsHexString(string s)
        {
            // 文字列sが16進文字列かどうか判定します。
            if (string.IsNullOrEmpty(s)) { return false; }

            foreach (char c in s)
            {
                if (!Uri.IsHexDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        //表示領域高さ
        private void BGC_H_TextChanged(object sender, TextChangedEventArgs e)
        {
            var isNumeric = int.TryParse(BGC_H.Text, out int n);

            if (isNumeric == true && vw != null)
            {

                vw.Height = int.Parse(BGC_H.Text) + 37;
            }
        }

        //表示領域幅
        private void BGC_W_TextChanged(object sender, TextChangedEventArgs e)
        {
            var isNumeric = int.TryParse(BGC_W.Text, out int n);

            if (isNumeric == true && vw != null)
            {

                    vw.Width = int.Parse(BGC_W.Text) + 6;
            }
        }

        //SE1選択
        private void SE1_Select_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "MP3ファイル (*.mp3)|*.mp3|WAVファイル (*.wav)|*.wav";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                // 選択されたファイル名 (ファイルパス) をTextblockに表示
                SE1_path.Text = dialog.FileName;
            }
        }

        //SE2選択
        private void SE2_Select_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "MP3ファイル (*.mp3)|*.mp3|WAVファイル (*.wav)|*.wav";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                // 選択されたファイル名 (ファイルパス) をTextblockに表示
                SE2_path.Text = dialog.FileName;
            }
        }

        //フォント選び
        private void FontSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (vw != null)
            {
                //TTG_viewer vw = Owner as TTG_viewer;
                vw.message.FontFamily = new FontFamily(Convert.ToString(FontSelect.SelectedItem));
            }
        }

        //フォントサイズ指定
        private void FontSize_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (vw != null)
            {
                double n;
                var w = Font_Size.Text;
                var isNumeric = double.TryParse(w, out n);

                if (isNumeric == true)
                {
                    //TTG_viewer vw = Owner as TTG_viewer;
                    if (Convert.ToInt32(w) > 0)
                    {
                        vw.message.FontSize = Convert.ToDouble(w);
                    }
                }
            }
        }

        //メッセージ表示領域のガイド
        private void MessageGideActive_Click(object sender, RoutedEventArgs e)
        {
            if (MessageGideActive.IsChecked == true)
            {
                vw.MessageGide.BorderThickness = new Thickness(2);
            }
            else
            {
                vw.MessageGide.BorderThickness = new Thickness(0);
            }
        }

        //メッセージの基点xy指定
        private void Message_x_y_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vw != null)
            {
                double n, m;
                var x = Message_x.Text;
                var y = Message_y.Text;

                var isNumeric1 = double.TryParse(x, out n);
                var isNumeric2 = double.TryParse(y, out m);

                if (isNumeric1 == true && isNumeric2 == true)
                {
                    //TTG_viewer vw = Owner as TTG_viewer;

                    Thickness margin = new Thickness(Convert.ToDouble(x), Convert.ToDouble(y), 0, 0);
                    vw.MessageGrid.Margin = margin;
                }
            }
        }
        //メッセージ表示幅
        private void Message_w_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vw != null)
            {
                double n;
                var h = Message_w.Text;
                var isNumeric = double.TryParse(h, out n);

                if (isNumeric == true)
                {
                    //TTG_viewer vw = Owner as TTG_viewer;

                    vw.MessageGrid.Width = Convert.ToDouble(h);
                }
            }
        }

        //画像1のON,OFF
        private void I1Active_Click(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBox = (CheckBox)sender;
            bool? check = CheckBox.IsChecked;

            //ビューウィンドウの呼び出し
            //TTG_viewer vw = Owner as TTG_viewer;

            if (check != true)
            {
                vw.Image1.Width = 0;
                vw.Image1.Height = 0;
            }
            else
            {
                vw.Image1.Width = Convert.ToDouble(Image1_w.Text);
                vw.Image1.Height = Convert.ToDouble(Image1_h.Text);

            }
        }

        //背面画像(画像1)のパス取得
        private void Image1Select_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "PNGファイル (*.png)|*.png|JPGファイル (*.jpg)|*.jpg|BMPファイル (*.bmp)|*.bmp";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                // 選択されたファイル名 (ファイルパス) をTextblockに表示
                Image1_path.Text = dialog.FileName;

                // ビューワへ画像パス反映
                //TTG_viewer vw = Owner as TTG_viewer;
                if (vw != null)
                {
                    vw.Image1.Source = Image1_viewer.Source;
                }
            }
        }

        //画像1の基点xy指定
        private void Image1_x_y_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vw != null)
            {
                double n, m;
                var x = Image1_x.Text;
                var y = Image1_y.Text;

                var isNumeric1 = double.TryParse(x, out n);
                var isNumeric2 = double.TryParse(y, out m);

                if (isNumeric1 == true && isNumeric2 == true)
                {
                    //TTG_viewer vw = Owner as TTG_viewer;

                    Thickness margin = new Thickness(Convert.ToDouble(x), Convert.ToDouble(y), 0, 0);
                    vw.Image1.Margin = margin;
                }
            }
        }

        //画像1の高さ指定
        private void Image1_h_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vw != null && I1Active.IsChecked == true)
            {
                double n;
                var h = Image1_h.Text;
                var isNumeric = double.TryParse(h, out n);

                if (isNumeric == true)
                {
                    //TTG_viewer vw = Owner as TTG_viewer;

                    vw.Image1.Height = Convert.ToDouble(h);
                }
            }
        }
        //画像1の幅さ指定
        private void Image1_w_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vw != null && I1Active.IsChecked == true)
            {
                double n;
                var w = Image1_w.Text;
                var isNumeric = double.TryParse(w, out n);

                if (isNumeric == true)
                {
                    //TTG_viewer vw = Owner as TTG_viewer;

                    vw.Image1.Width = Convert.ToDouble(w);
                }
            }
        }

        //画像2のON,OFF
        private void I2Active_Click(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBox = (CheckBox)sender;
            bool? check = CheckBox.IsChecked;

            //ビューウィンドウの呼び出し
            //TTG_viewer vw = Owner as TTG_viewer;

            if (check != true)
            {
                vw.Image2.Width = 0;
                vw.Image2.Height = 0;
            }
            else
            {
                vw.Image2.Width = Convert.ToDouble(Image2_w.Text);
                vw.Image2.Height = Convert.ToDouble(Image2_h.Text);

            }
        }

        //前面画像(画像2)のパス取得
        private void Image2Select_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "PNGファイル (*.png)|*.png|JPGファイル (*.jpg)|*.jpg|BMPファイル (*.bmp)|*.bmp";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                // 選択されたファイル名 (ファイルパス) をTextblockに表示
                Image2_path.Text = dialog.FileName;

                // ビューワへ画像パス反映
                //TTG_viewer vw = Owner as TTG_viewer;
                if (vw != null)
                {
                    vw.Image2.Source = Image2_viewer.Source;
                }
            }
        }

        //画像2の基点xy指定
        private void Image2_x_y_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vw != null)
            {
                double n, m;
                var x = Image2_x.Text;
                var y = Image2_y.Text;

                var isNumeric1 = double.TryParse(x, out n);
                var isNumeric2 = double.TryParse(y, out m);

                if (isNumeric1 == true && isNumeric2 == true)
                {
                    //TTG_viewer vw = Owner as TTG_viewer;

                    Thickness margin = new Thickness(Convert.ToDouble(x), Convert.ToDouble(y), 0, 0);
                    vw.Image2.Margin = margin;
                }
            }
        }

        //画像2の高さ指定
        private void Image2_h_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vw != null && I2Active.IsChecked == true)
            {
                double n;
                var h = Image2_h.Text;
                var isNumeric = double.TryParse(h, out n);

                if (isNumeric == true)
                {
                    //TTG_viewer vw = Owner as TTG_viewer;

                    vw.Image2.Height = Convert.ToDouble(h);
                }
            }
        }

        //画像2の幅さ指定
        private void Image2_w_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vw != null && I2Active.IsChecked == true)
            {
                double n;
                var w = Image2_w.Text;
                var isNumeric = double.TryParse(w, out n);

                if (isNumeric == true)
                {
                    //TTG_viewer vw = Owner as TTG_viewer;

                    vw.Image2.Width = Convert.ToDouble(w);
                }
            }
        }

        //画像3のON,OFF
        private void I3Active_Click(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBox = (CheckBox)sender;
            bool? check = CheckBox.IsChecked;

            //ビューウィンドウの呼び出し
            //TTG_viewer vw = Owner as TTG_viewer;

            if (check != true)
            {
                vw.Image3.Width = 0;
                vw.Image3.Height = 0;
            }
            else
            {
                vw.Image3.Width = Convert.ToDouble(Image3_w.Text);
                vw.Image3.Height = Convert.ToDouble(Image3_h.Text);

            }
        }

        //最前面画像(画像3)のパス取得
        private void Image3Select_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "PNGファイル (*.png)|*.png|JPGファイル (*.jpg)|*.jpg|BMPファイル (*.bmp)|*.bmp";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                // 選択されたファイル名 (ファイルパス) をTextblockに表示
                Image3_path.Text = dialog.FileName;

                // ビューワへ画像パス反映
                //TTG_viewer vw = Owner as TTG_viewer;
                if (vw != null)
                {
                    vw.Image3.Source = Image3_viewer.Source;
                }
            }
        }

        //画像3の基点xy指定
        private void Image3_x_y_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vw != null)
            {
                double n, m;
                var x = Image3_x.Text;
                var y = Image3_y.Text;

                var isNumeric1 = double.TryParse(x, out n);
                var isNumeric2 = double.TryParse(y, out m);

                if (isNumeric1 == true && isNumeric2 == true)
                {
                    //TTG_viewer vw = Owner as TTG_viewer;

                    Thickness margin = new Thickness(Convert.ToDouble(x), Convert.ToDouble(y), 0, 0);
                    vw.Image3.Margin = margin;
                }
            }
        }

        //画像3の高さ指定
        private void Image3_h_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vw != null && I3Active.IsChecked == true)
            {
                double n;
                var h = Image3_h.Text;
                var isNumeric = double.TryParse(h, out n);

                if (isNumeric == true)
                {
                    //TTG_viewer vw = Owner as TTG_viewer;

                    vw.Image3.Height = Convert.ToDouble(h);
                }
            }
        }

        //画像3の幅さ指定
        private void Image3_w_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vw != null && I3Active.IsChecked == true)
            {
                double n;
                var w = Image3_w.Text;
                var isNumeric = double.TryParse(w, out n);

                if (isNumeric == true)
                {
                    //TTG_viewer vw = Owner as TTG_viewer;

                    vw.Image3.Width = Convert.ToDouble(w);
                }
            }
        }

        //設定項目の最小化
        private void SettingMinButton_Click(object sender, RoutedEventArgs e)
        {
            string smb = Convert.ToString(SettingMinButton.Content);
            if (smb == "設定一覧 ▼")
            {
                TTG_setting_window.MinHeight = 305;
                TTG_setting_window.MaxHeight = double.PositiveInfinity;
                SettingGrid.Height = new GridLength(1.0, GridUnitType.Star);
                TTG_setting_window.Height = winSize;
                SettingMinButton.Content = "設定一覧 ▲";
            }
            else
            {
                winSize = TTG_setting_window.Height;
                SettingGrid.Height = new GridLength(0, GridUnitType.Star);
                TTG_setting_window.Height =230;
                TTG_setting_window.MinHeight = 230;
                TTG_setting_window.MaxHeight = 230;
                SettingMinButton.Content = "設定一覧 ▼";
            }
        }


        //タイマータイプによる選択肢のグレーアウト
        private void MessageType1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageType1_IsEnabled();
        }

        private void MessageType2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageType2_IsEnabled();
        }

        private void MessageType3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageType3_IsEnabled();
        }

        public void MessageType1_IsEnabled()
        {
            int SelectType = MessageType1.SelectedIndex;
            if (SelectType == 0 && vw != null)
            {
                Timer1.IsEnabled = false;
                Timer1.Text = "0";
                TimeRefText1.Text = "";
                TypeText1.Text = "";
            }
            else if (SelectType == 1 && vw != null)
            {
                Timer1.IsEnabled = true;
                TimeRefText1.Text = "から";
                TypeText1.Text = "分毎";
            }
        }

        public void MessageType2_IsEnabled()
        {
            int SelectType = MessageType2.SelectedIndex;
            if (SelectType == 0 && vw != null)
            {
                Timer2.IsEnabled = false;
                Timer2.Text = "0";
                TimeRefText2.Text = "";
                TypeText2.Text = "";
            }
            else if (SelectType == 1 && vw != null)
            {
                Timer2.IsEnabled = true;
                TimeRefText2.Text = "から";
                TypeText2.Text = "分毎";
            }
        }

        public void MessageType3_IsEnabled()
        {
            int SelectType = MessageType3.SelectedIndex;
            if (SelectType == 0 && vw != null)
            {
                Timer3.IsEnabled = false;
                Timer3.Text = "0";
                TimeRefText3.Text = "";
                TypeText3.Text = "";
            }
            else if (SelectType == 1 && vw != null)
            {
                Timer3.IsEnabled = true;
                TimeRefText3.Text = "から";
                TypeText3.Text = "分毎";
            }
        }


        //動作開始&停止
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

            ////////////////////////////////////////////////////////////////
            ////ボタン押下時に「開始」の場合の処理//////////////////////////
            ////テロップ初期化                    //////////////////////////
            ////////////////////////////////////////////////////////////////
            if (Convert.ToString(StartButton.Content) == "動作開始")
            {
                //開始ボタンを「停止」表記に変える
                StartButton.Content = "動作停止";
                StartButton.Foreground = new SolidColorBrush(Colors.Red);

                //タイマーを赤字に変更
                NowTime.Foreground = new SolidColorBrush(Colors.Red);

                //開始時の処理
                Trp_set_on();

                TestButton.IsEnabled = false;

                //タイマー起動開始　モード:1へ移行
                T_mode = 1;


            }

            ////////////////////////////////////////////////////////////////
            ////ボタン押下時に「停止」の場合の処理//////////////////////////
            ////////////////////////////////////////////////////////////////
            else
            {
                StartButton.Content = "動作開始";
                StartButton.Foreground = new SolidColorBrush(Colors.Black);

                //タイマーを黒字に変更
                NowTime.Foreground = new SolidColorBrush(Colors.Black);

                //停止時の処理
                Trp_set_off();

                TestButton.IsEnabled = true;

                //タイマーモードを戻す
                T_mode = 0;
            }

        }

        //テスト再生開始&停止
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            ////////////////////////////////////////////////////////////////
            ////ボタン押下時に「開始」の場合の処理//////////////////////////
            ////テロップ初期化                    //////////////////////////
            ////////////////////////////////////////////////////////////////
            if (Convert.ToString(TestButton.Content) == "テスト開始")
            {
                //テストボタンを「停止」表記に変える
                TestButton.Content = "テスト中断";
                TestButton.Foreground = new SolidColorBrush(Colors.Red);

                //開始時の処理
                Trp_set_on();

                StartButton.IsEnabled = false;

                if (MessageActive1.IsChecked == true)
                {
                    Message_f[0] = Message_f.Max() + 1;
                    Message_s[0] = TimeToMessage(Message1.Text, 1);
                }

                if (MessageActive2.IsChecked == true)
                {
                    Message_f[1] = Message_f.Max() + 1;
                    Message_s[1] = TimeToMessage(Message2.Text, 1);
                }

                if (MessageActive3.IsChecked == true)
                {
                    Message_f[2] = Message_f.Max() + 1;
                    Message_s[2] = TimeToMessage(Message3.Text, 1);
                }


                //タイマー起動開始　モード:1へ移行
                T_mode = 2;
                Test_f = true;

            }

            ////////////////////////////////////////////////////////////////
            ////ボタン押下時に「停止」の場合の処理//////////////////////////
            ////////////////////////////////////////////////////////////////
            else
            {
                //テストボタンを「停止」表記に変える
                TestButton.Content = "テスト開始";
                TestButton.Foreground = new SolidColorBrush(Colors.Black);

                //停止時の処理
                Trp_set_off();


                StartButton.IsEnabled = true;

                //タイマーモードを戻す
                T_mode = 0;
                Test_f = false;
            }
        }

        //開始時の処理
        private void Trp_set_on()
        {
            //項目グレーアウト
            Timer1.IsEnabled = false;
            Timer2.IsEnabled = false;
            Timer3.IsEnabled = false;
            MessageActive1.IsEnabled = false; //項目全グレーアウト

            //メッセージのトリガフラグを初期化
            Message_f[0] = 0;
            Message_f[1] = 0;
            Message_f[2] = 0;

            //メッセージの表示カウンター初期化
            Message_c[0] = 0;
            Message_c[1] = 0;
            Message_c[2] = 0;

            //スライドイン初期位置を設定
            SlideInSetting();

            //スライドアウト方向取得
            SlideOutType = OutDirection.SelectedIndex;
        }
        //停止時の処理
        private void Trp_set_off()
        {
            //メッセージのトリガフラグを初期化
            Message_f[0] = 0;
            Message_f[1] = 0;
            Message_f[2] = 0;

            //メッセージの表示カウンター初期化
            Message_c[0] = 0;
            Message_c[1] = 0;
            Message_c[2] = 0;

            MessageType1_IsEnabled();
            MessageType2_IsEnabled();
            MessageType3_IsEnabled();

            Canvas.SetLeft(vw.message, 0);

            ParametersUpdate();

            MessageActive1.IsEnabled = true;//項目全グレーアウト解除
        }

        //スライドイン初期位置設定
        private void SlideInSetting()
        {

            //スライドイン方向取得
            SlideInType = InDirection.SelectedIndex;

            //スライド方向補正変数
            Double slide_x = 0, slide_y = 0;

            //スライド方向による初期位置補正変数
            if (SlideInType == 0)
            {
                slide_x = Convert.ToDouble(BGC_W.Text);
                slide_y = 0;
            }
            else if (SlideInType == 1)
            {
                slide_x = -1 * Convert.ToDouble(BGC_W.Text);
                slide_y = 0;
            }
            else if (SlideInType == 2)
            {
                slide_x = 0;
                slide_y = Convert.ToDouble(BGC_H.Text);
            }
            else if (SlideInType == 3)
            {
                slide_x = 0;
                slide_y = -1 * Convert.ToDouble(BGC_H.Text);
            }

            Double image1_0_x = Convert.ToDouble(Image1_x.Text) + slide_x;
            Double image1_0_y = Convert.ToDouble(Image1_y.Text) + slide_y;

            Double image2_0_x = Convert.ToDouble(Image2_x.Text) + slide_x;
            Double image2_0_y = Convert.ToDouble(Image2_y.Text) + slide_y;

            Double image3_0_x = Convert.ToDouble(Image3_x.Text) + slide_x;
            Double image3_0_y = Convert.ToDouble(Image3_y.Text) + slide_y;

            Double Message_0_x = Convert.ToDouble(Message_x.Text) + slide_x;
            Double Message_0_y = Convert.ToDouble(Message_y.Text) + slide_y;


            //メッセージテロップ位置初期化
            vw.Image1.Margin = new Thickness(image1_0_x, image1_0_y, 0, 0);
            vw.Image2.Margin = new Thickness(image2_0_x, image2_0_y, 0, 0);
            vw.Image3.Margin = new Thickness(image3_0_x, image3_0_y, 0, 0);
            vw.MessageGrid.Margin = new Thickness(Message_0_x, Message_0_y, 0, 0);

            //流れるメッセージ初期位置へ
            Canvas.SetLeft(vw.message, Convert.ToDouble(Message_w.Text));
        }

        //パラメータ反映
        private void ParametersUpdate()
        {

            string exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)+"/Resource/";

            string ip = "";

            //背景色
            if (Encoding.GetEncoding("Shift_JIS").GetByteCount(BGC.Text) == 7)
            {
                string BGC_16 = BGC.Text.Substring(1, 6);
                if (BGC.Text.Substring(0, 1) == "#" && IsHexString(BGC_16) == true)
                {
                    R_slider.Value = Int32.Parse(BGC_16.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    G_slider.Value = Int32.Parse(BGC_16.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    B_slider.Value = Int32.Parse(BGC_16.Substring(4), System.Globalization.NumberStyles.HexNumber);
                }
            }

            vw.TTG_viewer_window.Background = BGC_sample.Background;


            //表示領域の幅
            vw.TTG_viewer_window.Height = int.Parse(BGC_H.Text) + 37;
            vw.TTG_viewer_window.Width = int.Parse(BGC_W.Text) + 6;

            ip = SE1_path.Text;
            //SE1_URL取得
            if (Regex.IsMatch(ip, ".[.]wav|mp3$")==false || System.IO.File.Exists(ip) == false) { SE1_path.Text = exePath + "SE1.mp3"; }


            ip = SE2_path.Text;
            //SE2_URL取得
            if (Regex.IsMatch(ip, ".[.]wav|mp3$") == false || System.IO.File.Exists(ip) == false) { SE2_path.Text = exePath + "SE2.mp3"; }

            //メッセージ初期化
            vw.message.Text = "Test123てすとテストﾃｽﾄ";

            //メッセージフォント
            vw.message.FontFamily = new FontFamily(Convert.ToString(FontSelect.SelectedItem));
            //メッセージサイズ
            vw.message.FontSize = Convert.ToDouble(Font_Size.Text);

            //メッセージカラー
            if (Encoding.GetEncoding("Shift_JIS").GetByteCount(TextC.Text) == 7)
            {
                string TextC_16 = TextC.Text.Substring(1, 6);
                if (TextC.Text.Substring(0, 1) == "#" && IsHexString(TextC_16) == true)
                {
                    R_slider_T.Value = Int32.Parse(TextC_16.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    G_slider_T.Value = Int32.Parse(TextC_16.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    B_slider_T.Value = Int32.Parse(TextC_16.Substring(4), System.Globalization.NumberStyles.HexNumber);
                }
            }

            vw.message.Foreground = TextC_sample.Background;

            if (MessageGideActive.IsChecked == true)
            {
                vw.MessageGide.BorderThickness = new Thickness(2);
            }
            else
            {
                vw.MessageGide.BorderThickness = new Thickness(0);
            }

            //メッセージ基点xy
            vw.MessageGrid.Margin = new Thickness(Convert.ToDouble(Message_x.Text), Convert.ToDouble(Message_y.Text), 0, 0);

            //メッセージ表示幅
            vw.MessageGrid.Width = Convert.ToDouble(Message_w.Text);


            //背景画像ソース
            //背景画像_Image1
            ip = Image1_path.Text;
            if (Regex.IsMatch(ip, ".[.]jpg|bmp|png$")== false || System.IO.File.Exists(ip) == false)
            {
                vw.Image1.Source = new BitmapImage(new Uri(exePath + "Image1.png", UriKind.Absolute));
                Image1_path.Text = Convert.ToString(vw.Image1.Source);

            }
            else
            {
                vw.Image1.Source = new BitmapImage(new Uri(ip, UriKind.Absolute));
            }
            //前面画像_Image2
            ip = Image2_path.Text;
            if (Regex.IsMatch(ip, ".[.]jpg|bmp|png$") == false || System.IO.File.Exists(ip) == false)
            {
                vw.Image2.Source = new BitmapImage(new Uri(exePath + "Image2.png", UriKind.Absolute));
                Image2_path.Text = Convert.ToString(vw.Image2.Source);

            }
            else
            {
                vw.Image2.Source = new BitmapImage(new Uri(ip, UriKind.Absolute));
            }
            //最前面画像_Image3
            ip = Image3_path.Text;
            if (Regex.IsMatch(ip, ".[.]jpg|bmp|png$") == false || System.IO.File.Exists(ip) == false)
            {
                vw.Image3.Source = new BitmapImage(new Uri(exePath + "Image3.png", UriKind.Absolute));
                Image3_path.Text = Convert.ToString(vw.Image3.Source);

            }
            else
            {
                vw.Image3.Source = new BitmapImage(new Uri(ip, UriKind.Absolute));
            }

            //背景画像基点xy
            vw.Image1.Margin = new Thickness(Convert.ToDouble(Image1_x.Text), Convert.ToDouble(Image1_y.Text), 0, 0);
            //背景画像幅高さ
            if (I1Active.IsChecked != true)
            {
                vw.Image1.Width = 0;
                vw.Image1.Height = 0;
            }
            else
            {
                vw.Image1.Width = Convert.ToDouble(Image1_w.Text);
                vw.Image1.Height = Convert.ToDouble(Image1_h.Text);



            }

            //前面画像基点xy
            vw.Image2.Margin = new Thickness(Convert.ToDouble(Image2_x.Text), Convert.ToDouble(Image2_y.Text), 0, 0);
            //前面画像幅高さ
            if (I2Active.IsChecked != true)
            {
                vw.Image2.Width = 0;
                vw.Image2.Height = 0;
            }
            else
            {
                vw.Image2.Width = Convert.ToDouble(Image2_w.Text);
                vw.Image2.Height = Convert.ToDouble(Image2_h.Text);

            }

            //最前面画像基点xy
            vw.Image3.Margin = new Thickness(Convert.ToDouble(Image3_x.Text), Convert.ToDouble(Image3_y.Text), 0, 0);
            //最前面画像幅高さ
            if (I3Active.IsChecked != true)
            {
                vw.Image3.Width = 0;
                vw.Image3.Height = 0;
            }
            else
            {
                vw.Image3.Width = Convert.ToDouble(Image3_w.Text);
                vw.Image3.Height = Convert.ToDouble(Image3_h.Text);
            }
        }

        //メッセージの中での時間置き換え文字列を処理
        public string TimeToMessage(string str, int m)
        {
            string str2="";

            if (Regex.IsMatch(str, "^*<CT_HH:mm>"))
            {
                if (m == 1) { str2 = NextTime1.Text; }
                else if (m == 2) { str2 = NextTime2.Text; }
                else if (m == 3) { str2 = NextTime3.Text; }
                str=str.Replace("<CT_HH:mm>",str2);
            }

            if (Regex.IsMatch(str, "^*<CT_H>"))
            {
                if (m == 1) { str2 = Convert.ToString(Convert.ToInt32( NextTime1.Text.Substring(0,2) ) ); }
                else if (m == 2) { str2 = Convert.ToString(Convert.ToInt32(NextTime2.Text.Substring(0,2))); }
                else if (m == 3) { str2 = Convert.ToString(Convert.ToInt32(NextTime3.Text.Substring(0,2))); }
                str=str.Replace("<CT_H>", str2);
            }

            if (Regex.IsMatch(str, "^*<CT_H/m>"))
            {
                if (m == 1) { str2 = Convert.ToString(Convert.ToInt32(NextTime1.Text.Substring(3))); }
                else if (m == 2) { str2 = Convert.ToString(Convert.ToInt32(NextTime2.Text.Substring(3))); }
                else if (m == 3) { str2 = Convert.ToString(Convert.ToInt32(NextTime3.Text.Substring(3))); }
                str=str.Replace("<CT_H/m>", str2);
            }

            if (Regex.IsMatch(str, "^*<ET_HH:mm>"))
            {
                if (m == 1) { str2 = Time_plus_m("00:00",Convert.ToString(Convert.ToInt32(Timer1.Text) * Message_c[m - 1])); }
                else if (m == 2) { str2 = Time_plus_m("00:00", Convert.ToString(Convert.ToInt32(Timer2.Text) * Message_c[m - 1])); }
                else if (m == 3) { str2 = Time_plus_m("00:00", Convert.ToString(Convert.ToInt32(Timer3.Text) * Message_c[m - 1])); }
                str = str.Replace("<ET_HH:mm>", str2);
            }

            if (Regex.IsMatch(str, "^*<ET_H>"))
            {
                if (m == 1) { str2 = Convert.ToString(Convert.ToInt32(Time_plus_m("00:00", Convert.ToString(Convert.ToInt32(Timer1.Text) * Message_c[m - 1])).Substring(0,2))); }
                else if (m == 2) { str2 = Convert.ToString(Convert.ToInt32(Time_plus_m("00:00", Convert.ToString(Convert.ToInt32(Timer2.Text) * Message_c[m - 1])).Substring(0, 2))); }
                else if (m == 3) { str2 = Convert.ToString(Convert.ToInt32(Time_plus_m("00:00", Convert.ToString(Convert.ToInt32(Timer3.Text) * Message_c[m - 1])).Substring(0, 2))); }
                str = str.Replace("<ET_H>", str2);
            }

            if (Regex.IsMatch(str, "^*<ET_H/m>"))
            {
                if (m == 1) { str2 = Convert.ToString(Convert.ToInt32(Time_plus_m("00:00", Convert.ToString(Convert.ToInt32(Timer1.Text) * Message_c[m - 1])).Substring(3))); }
                else if (m == 2) { str2 = Convert.ToString(Convert.ToInt32(Time_plus_m("00:00", Convert.ToString(Convert.ToInt32(Timer2.Text) * Message_c[m - 1])).Substring(3))); }
                else if (m == 3) { str2 = Convert.ToString(Convert.ToInt32(Time_plus_m("00:00", Convert.ToString(Convert.ToInt32(Timer3.Text) * Message_c[m - 1])).Substring(3))); }
                str = str.Replace("<ET_H/m>", str2);
            }

            if (Regex.IsMatch(str, "^*<ET_m>"))
            {
                if (m == 1) { str2 = Convert.ToString(Convert.ToInt32(Timer1.Text) * Message_c[m - 1]); }
                else if (m == 2) { str2 = Convert.ToString(Convert.ToInt32(Timer2.Text) * Message_c[m - 1]); }
                else if (m == 3) { str2 = Convert.ToString(Convert.ToInt32(Timer3.Text) * Message_c[m - 1]); }
                str = str.Replace("<ET_m>", str2);
            }

            return str;
        }

        //string型の"00:00"と分の足し算をしてstring型の"00:00"形式で返す
        public string Time_plus_m(string T, string minutes)
        {
            int H, m, M;

            string HH,mm;

            string t;

            //時間の入力が合っているか判定「--:--」形式、minutesは数字か判定
            if (Regex.IsMatch(T, "^[0-9][0-9]:[0-9][0-9]$") &&  double.TryParse(minutes, out double n))
            {
                H = Convert.ToInt32(T.Substring(0, 2));
                m = Convert.ToInt32(T.Substring(3, 2));

                M = Convert.ToInt32(minutes);

                H = (H + ((m + M) / 60)) % 24; //時
                m = (m + M) % 60;//分


                //時&分が2桁あるか判定し、1桁の場合に先頭0を追加する。
                if (H < 10)
                {
                    HH = "0" + Convert.ToString(H);
                }
                else
                {
                    HH = Convert.ToString(H);
                }

                if (m < 10)
                {
                    mm = "0" + Convert.ToString(m);
                }
                else
                {
                    mm = Convert.ToString(m);
                }

                t = HH + ":" + mm;
            }
            else { t = "--:--"; }

            return t;
        }

        //メッセージ有効チェックボックスの変化時の処理
        private void MessageActive_Click(object sender, RoutedEventArgs e)
        {
            Message_Setting_Change();
            ActiveButtonCheck();
        }

        private void ActiveButtonCheck()
        {
            if (MessageActive1.IsChecked == true || MessageActive2.IsChecked == true || MessageActive3.IsChecked == true)
            {
                TestButton.IsEnabled = true;
                StartButton.IsEnabled = true;
            }
            else
            {
                TestButton.IsEnabled = false;
                StartButton.IsEnabled = false;
            }
        }

        //メッセージタイマー変更時の処理
        private void Time_TextChanged(object sender, TextChangedEventArgs e)
        {
            Message_Setting_Change();
        }

        //有効なメッセージなら次の時刻を表示
        private void Message_Setting_Change()
        {
            if (MessageActive1.IsChecked == true)
            {
                NextTime1.Text = Time_plus_m(TimeRef1.Text, Timer1.Text);
            }
            else
            {
                NextTime1.Text = "--:--";
            }

            if (MessageActive2.IsChecked == true)
            {
                NextTime2.Text = Time_plus_m(TimeRef2.Text, Timer2.Text);
            }
            else
            {
                NextTime2.Text = "--:--";
            }

            if (MessageActive3.IsChecked == true)
            {
                NextTime3.Text = Time_plus_m(TimeRef3.Text, Timer3.Text);
            }
            else
            {
                NextTime3.Text = "--:--";
            }
        }

        //音声再生
        private void Sound_play(string sound_path)
        {
            _mediaPlayer.URL = sound_path;// mp3も使用可能
            _mediaPlayer.controls.play();
        }


        //XMLファイルから設定読み込み
        private void SettingsLoad(int f)
        {

            string exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //保存先のファイル名
            string fileName = exePath + "/Mysettings/myset" + f + ".xml";

            if (System.IO.File.Exists(fileName))
            {
                //XmlSerializerオブジェクトを作成
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
                //読み込むファイルを開く
                System.IO.StreamReader sr = new System.IO.StreamReader(fileName, new System.Text.UTF8Encoding(false));
                //XMLファイルから読み込み、逆シリアル化する
                Settings obj = (Settings)serializer.Deserialize(sr);
                //ファイルを閉じる
                sr.Close();

                string ip = "";

                winSize = obj.winSize;

                MessageActive1.IsChecked = obj.MessageActive1;
                MessageActive2.IsChecked = obj.MessageActive2;
                MessageActive3.IsChecked = obj.MessageActive3;

                MessageType1.SelectedIndex = obj.MessageType1;
                MessageType2.SelectedIndex = obj.MessageType2;
                MessageType3.SelectedIndex = obj.MessageType2;


                TimeRef1.Text = obj.TimeRef1;
                TimeRef2.Text = obj.TimeRef2;
                TimeRef3.Text = obj.TimeRef3;

                Timer1.Text = obj.Timer1;
                Timer2.Text = obj.Timer2;
                Timer3.Text = obj.Timer3;

                Message1.Text = obj.Message1;
                Message2.Text = obj.Message2;
                Message3.Text = obj.Message3;

                InDirection.SelectedIndex = obj.InDirection;
                InSpeed.Text = obj.InSpeed;

                OutDirection.SelectedIndex = obj.OutDirection;
                OutSpeed.Text = obj.OutSpeed;
                SlideSpeed.Text = obj.SlideSpeed;
                MessageGideActive.IsChecked = obj.MessageGideActive;

                Message_x.Text = obj.Message_x;
                Message_y.Text = obj.Message_y;
                Message_w.Text = obj.Message_w;

                FontSelect.SelectedIndex = obj.FontSelect;
                Font_Size.Text = obj.Font_Size;

                TextC.Text = obj.TextC;

                BGC.Text = obj.BGC;
                BGC_H.Text = obj.BGC_H;
                BGC_W.Text = obj.BGC_W;

                //SE1_URL取得
                ip = obj.SE1_path;
                if (Regex.IsMatch(ip, ".[.]wav|mp3$") == false || System.IO.File.Exists(ip) == false) { ip = exePath + "/Resource/SE1.mp3"; }
                SE1Active.IsChecked = obj.SE1Active;
                SE1_path.Text = ip;

                //SE2_URL取得
                ip = obj.SE2_path;
                if (Regex.IsMatch(ip, ".[.]wav|mp3$") == false || System.IO.File.Exists(ip) == false) { ip = exePath + "/Resource/SE2.mp3"; }
                SE2Active.IsChecked = obj.SE2Active;
                SE2_path.Text = ip;



                ip = obj.Image1_path;
                if (Regex.IsMatch(ip, ".[.]jpg|bmp|png$") == false || System.IO.File.Exists(ip) == false) {ip = exePath + "/Resource/Image1.png"; }

                I1Active.IsChecked = obj.I1Active;
                Image1_path.Text =ip;
                vw.Image1.Source = new BitmapImage(new Uri(ip));
                Image1_x.Text = obj.Image1_x;
                Image1_y.Text = obj.Image1_y;
                Image1_w.Text = obj.Image1_w;
                Image1_h.Text = obj.Image1_h;


                ip = obj.Image2_path;
                if (Regex.IsMatch(ip, ".[.]jpg|bmp|png$") == false || System.IO.File.Exists(ip) == false) { ip = exePath + "/Resource/Image2.png"; }

                I2Active.IsChecked = obj.I2Active;
                Image2_path.Text = ip;
                vw.Image2.Source = new BitmapImage(new Uri(ip));
                Image2_x.Text = obj.Image2_x;
                Image2_y.Text = obj.Image2_y;
                Image2_w.Text = obj.Image2_w;
                Image2_h.Text = obj.Image2_h;

                ip = obj.Image3_path;
                if (Regex.IsMatch(ip, ".[.]jpg|bmp|png$") == false || System.IO.File.Exists(ip) == false) { ip = exePath + "/Resource/Image3.png"; }

                I3Active.IsChecked = obj.I3Active;
                Image3_path.Text = ip;
                vw.Image3.Source = new BitmapImage(new Uri(ip));
                Image3_x.Text = obj.Image3_x;
                Image3_y.Text = obj.Image3_y;
                Image3_w.Text = obj.Image3_w;
                Image3_h.Text = obj.Image3_h;
            }

        }

        //設定をXMLファイルに保存する
        public void SettingsSave(int f)
        {
            //保存先のファイル名
            string fileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +"/Mysettings/myset"+f+".xml";

            //保存するクラス(SampleClass)のインスタンスを作成
            Settings obj = new Settings();

            obj.winSize = winSize;

            obj.MessageActive1 = Convert.ToBoolean(MessageActive1.IsChecked);
            obj.MessageActive2 = Convert.ToBoolean(MessageActive2.IsChecked);
            obj.MessageActive3 = Convert.ToBoolean(MessageActive3.IsChecked);

            obj.MessageType1 = MessageType1.SelectedIndex;
            obj.MessageType2 = MessageType2.SelectedIndex;
            obj.MessageType3 = MessageType3.SelectedIndex;


            obj.TimeRef1 = TimeRef1.Text;
            obj.TimeRef2 = TimeRef2.Text;
            obj.TimeRef3 = TimeRef3.Text;

            obj.Timer1 = Timer1.Text;
            obj.Timer2 = Timer2.Text;
            obj.Timer3 = Timer3.Text;

            obj.Message1 = Message1.Text;
            obj.Message2 = Message2.Text;
            obj.Message3 = Message3.Text;

            obj.InDirection = InDirection.SelectedIndex;
            obj.InSpeed = InSpeed.Text;

            obj.OutDirection = OutDirection.SelectedIndex;
            obj.OutSpeed = OutSpeed.Text;
            obj.SlideSpeed = SlideSpeed.Text;

            obj.MessageGideActive = Convert.ToBoolean(MessageGideActive.IsChecked);

            obj.Message_x = Message_x.Text;
            obj.Message_y = Message_y.Text;
            obj.Message_w = Message_w.Text;

            obj.FontSelect = FontSelect.SelectedIndex;
            obj.Font_Size = Font_Size.Text;

            obj.TextC = TextC.Text;

            obj.BGC = BGC.Text;
            obj.BGC_H = BGC_H.Text;
            obj.BGC_W = BGC_W.Text;


            obj.SE1Active = Convert.ToBoolean(SE1Active.IsChecked);
            obj.SE1_path = SE1_path.Text;

            obj.SE2Active = Convert.ToBoolean(SE2Active.IsChecked);
            obj.SE2_path = SE2_path.Text;


            obj.I1Active = Convert.ToBoolean(I1Active.IsChecked);
            obj.Image1_path = Image1_path.Text;
            obj.Image1_x = Image1_x.Text;
            obj.Image1_y = Image1_y.Text;
            obj.Image1_w = Image1_w.Text;
            obj.Image1_h = Image1_h.Text;


            obj.I2Active = Convert.ToBoolean(I2Active.IsChecked);
            obj.Image2_path = Image2_path.Text;
            obj.Image2_x = Image2_x.Text;
            obj.Image2_y = Image2_y.Text;
            obj.Image2_w = Image2_w.Text;
            obj.Image2_h = Image2_h.Text;


            obj.I3Active = Convert.ToBoolean(I3Active.IsChecked);
            obj.Image3_path = Image3_path.Text;
            obj.Image3_x = Image3_x.Text;
            obj.Image3_y = Image3_y.Text;
            obj.Image3_w = Image3_w.Text;
            obj.Image3_h = Image3_h.Text;

            //XmlSerializerオブジェクトを作成
            //オブジェクトの型を指定する
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            //書き込むファイルを開く（UTF-8 BOM無し）
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName, false, new System.Text.UTF8Encoding(false));
            //シリアル化し、XMLファイルに保存する
            serializer.Serialize(sw, obj);
            //ファイルを閉じる
            sw.Close();
        }


        //音声テスト再生
        private void SE1_test_Click(object sender, RoutedEventArgs e)
        {
            Sound_play(SE1_path.Text);
        }
        private void SE2_test_Click(object sender, RoutedEventArgs e)
        {
            Sound_play(SE2_path.Text);
        }

    }

    //マイセット用のクラス
    public class Settings
    {
        public string MysetName;

        public double winSize;

        public bool MessageActive1;
        public bool MessageActive2;
        public bool MessageActive3;
        public int MessageType1;
        public int MessageType2;
        public int MessageType3;
        public string TimeRef1;
        public string TimeRef2;
        public string TimeRef3;
        public string Timer1;
        public string Timer2;
        public string Timer3;
        public string Message1;
        public string Message2;
        public string Message3;
        public int InDirection;
        public string InSpeed;
        public int OutDirection;
        public string OutSpeed;
        public string SlideSpeed;
        public bool MessageGideActive;
        public string Message_x;
        public string Message_y;
        public string Message_w;
        public int FontSelect;
        public string Font_Size;
        public string TextC;
        public string BGC;
        public string BGC_W;
        public string BGC_H;
        public bool SE1Active;
        public string SE1_path;
        public bool SE2Active;
        public string SE2_path;
        public bool I1Active;
        public string Image1_path;
        public string Image1_x;
        public string Image1_y;
        public string Image1_w;
        public string Image1_h;
        public bool I2Active;
        public string Image2_path;
        public string Image2_x;
        public string Image2_y;
        public string Image2_w;
        public string Image2_h;
        public bool I3Active;
        public string Image3_path;
        public string Image3_x;
        public string Image3_y;
        public string Image3_w;
        public string Image3_h;
    }

    //フォント名を使用言語に合わせる奴
    public class FontFamilyToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var v = value as FontFamily;
            var currentLang = XmlLanguage.GetLanguage(culture.IetfLanguageTag);
            return v.FamilyNames.FirstOrDefault(o => o.Key == currentLang).Value ?? v.Source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}
