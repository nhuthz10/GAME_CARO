using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;
using Caro.Properties;

namespace Caro
{
    public partial class frmPlayGame : Form
    {
        bool PvsP;
        bool PvsC;
        int CheDo;

        private StreamReader FR;

        string P1;
        string P2;
        
        frmPvsP P1vsP2 = null;
        frmPvsC PvsCOM = null;
 
        private List<Player> player;
        public List<Player> Player { get => player; set => player = value; }

        private int current;
        public int Current { get => current; set => current = value; }

        private List<List<Button>> matrix;
        public List<List<Button>> Matrix { get => matrix; set => matrix = value; }

        private Stack<Point> comeback;
       public Stack<Point> Comeback { get => comeback; set => comeback = value; }

        private Stack<Point> advance;
        public Stack<Point> Advance { get => advance; set => advance = value; }
       

        private long[] TanCong = new long[6] { 0, 64, 4096, 262144, 16777216, 1073741824 };

        private long[] PhongNgu = new long[6] { 0, 8, 512, 32768, 2097152, 134217728 };

        public void funData1(TextBox Player1) { P1 = Player1.Text; }
        public void funData2(TextBox Player2) { P2 = Player2.Text; }

        public frmPlayGame()
        {

            InitializeComponent();

            prcbCoolDown.Step = Cons.CoolDownStep;
            prcbCoolDown.Maximum = Cons.CoolDownTime;
            prcbCoolDown.Value = 0;

            tmCoolDown.Interval = Cons.CoolDownInterval;
        }


        private void btnQuitGame_Click(object sender, EventArgs e)
        {
            Quit();
        }

        private void frmPlayGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Do you want to exit the game ?", "Game Caro", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
                e.Cancel = true;
        }

        public void Quit()
        {
            this.Close();
        }

        void DrawChessBoard() {
            pnlChessBoard.Controls.Clear();
            Matrix = new List<List<Button>>();

            Button oldButton = new Button() { Width = 0, Location = new Point(0, 0) };
            for (int i = 0; i < Cons.ChessBoard_Height; i++) {

                Matrix.Add(new List<Button>());

                for (int j = 0; j <= Cons.ChessBoard_Width; j++) {
                    Button btn = new Button() {
                        Width = Cons.Chess_Width,
                        Height = Cons.Chess_Height,
                        Location = new Point(oldButton.Location.X + oldButton.Width, oldButton.Location.Y),
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Tag = i.ToString(),
                    };

                    btn.Click += Btn_Click;

                    pnlChessBoard.Controls.Add(btn);

                    Matrix[i].Add(btn);

                    oldButton = btn;
                }
                oldButton.Location = new Point(0, oldButton.Location.Y + Cons.Chess_Height);
                oldButton.Width = 0;
                oldButton.Height = 0;
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            Advance = new Stack<Point>();

            Button btn = sender as Button;
            if (btn.BackgroundImage != null)
                return;

            Mark(btn);
            Comeback.Push(GetChessPoint(btn));

            Current = Current == 1 ? 0 : 1;

            changePlayer();

            tmCoolDown.Start();
            prcbCoolDown.Value = 0;

            if (isEndGame(btn))
            {
                EndGame();

            }

            if (PvsC == true && Current == 0 && pnlChessBoard.Enabled==true)
            {
                Current = Current == 1 ? 0 : 1;
                changePlayer();
                StartComputer(btn);
            }   
        }

        private void Sound()
        {
            System.Media.SoundPlayer bloop = new System.Media.SoundPlayer(Application.StartupPath + "\\Sound\\bloop.wav");
            bloop.Play();
        }

        public bool Redo()
        {
            if (Advance.Count <= 0)
            {
                tmCoolDown.Stop();
                prcbCoolDown.Value = 0;
                return false;
            }


            prcbCoolDown.Value = 0;

            Point point = Advance.Peek();

            Comeback.Push(point);

            Point oldpoint = Advance.Pop();
            Button btn = Matrix[oldpoint.Y][oldpoint.X];

            btn.BackgroundImage = Player[Current].Mark;

            Current = Current == 1 ? 0 : 1;

            if (PvsC == true && Current == 0)
            {
                changePlayer();
                StartComputer(btn);
                Current = Current == 1 ? 0 : 1;
            }

            

            changePlayer();

            return true;
        }

        public bool Undo()
        {
            if (Comeback.Count <= 0)
            {
                tmCoolDown.Stop();
                prcbCoolDown.Value = 0;
                return false;
            }

            if (PvsC==true && Comeback.Count>1)
            {
                prcbCoolDown.Value = 0;
                tmCoolDown.Start();

                Point pointcom = Comeback.Pop();

                Button com = Matrix[pointcom.Y][pointcom.X];
                com.BackgroundImage = null;

                Point pointP = Comeback.Peek();

                Advance.Push(pointP);

                Point poin = Comeback.Pop();
                Button btnC = Matrix[poin.Y][poin.X];
                btnC.BackgroundImage = null;
                
                changePlayer();

                return true;
            }



            Point point = Comeback.Peek();

            Advance.Push(point);

            Point oldpoint = Comeback.Pop();
           

            Button btn = Matrix[oldpoint.Y][oldpoint.X];

           

            btn.BackgroundImage = null;

           
            Current = Current == 1 ? 0 : 1;
            changePlayer();

            return true;
        }

        void PlayervsPlayer(String P1,String P2) {
            PvsP = true;
            PvsC = false;
            if (PvsP == true)
            {
                CheDo = 1;
                this.Player = new List<Player>() {
                new Player(P1,Image.FromFile(Application.StartupPath+ "\\Resources\\1.jpg")),
                new Player(P2,Image.FromFile(Application.StartupPath+ "\\Resources\\2.jpg")),
                };

                Comeback = new Stack<Point>();
                int Rnum;
                Random rad = new Random();
                Rnum = rad.Next(0, 100);
                current = Rnum % 2;
                changePlayer();
            }
            
        }

        void PlayervsCom(String P1)
        {
            PvsC = true;
            PvsP = false;
            if (PvsC == true)
            {
                CheDo = 2;
                this.Player = new List<Player>() 
                {
                new Player("Computer",Image.FromFile(Application.StartupPath+ "\\Resources\\2.jpg")),
                new Player(P1,Image.FromFile(Application.StartupPath+ "\\Resources\\1.jpg")),
                };
                Comeback = new Stack<Point>();
                current = 1;
                changePlayer();              
            }
            
        }
        private void changePlayer()
        {
            txbPlayerName.Text = Player[current].Name;
            if (current == 0)
                pctbMark.Image = Image.FromFile(Application.StartupPath + "\\Resources\\player1.jpg");
            else
                pctbMark.Image = Image.FromFile(Application.StartupPath + "\\Resources\\player2.jpg");
        }
        public void StartComputer(Button btn)
        {
            Point point = PointCOM();

            btn = Matrix[point.X][point.Y];
            btn.BackgroundImage = Player[0].Mark;
            Sound();
            Comeback.Push(GetChessPoint(btn));
            if (isEndGame(btn))
            {
                EndGame();
            }
        }
        public Point PointCOM()
        {
            Button btn=new Button();
            Point ChessPoint = new Point();

            long DiemMax = 0;

            for (int i = 0; i < Cons.ChessBoard_Height; i++)
            {
                for (int j = 0; j < Cons.ChessBoard_Width; j++)
                {
                    if (Matrix[i][j].BackgroundImage == null)
                    {
                        long DiemTanCong = DiemTanCong_Doc(i, j) + DiemTanCong_Ngang(i, j) + DiemTanCong_Cheo(i, j) + DiemTanCong_CheoNguoc(i, j);
                        long DiemPhongNgu = DiemPhongNgu_Doc(i, j) + DiemPhongNgu_Ngang(i, j) + DiemPhongNgu_Cheo(i, j) + DiemPhongNgu_CheoNguoc(i, j);

                        long DiemTam = DiemTanCong > DiemPhongNgu ? DiemTanCong : DiemPhongNgu;
                        long DiemTong = (DiemTanCong + DiemPhongNgu) > DiemTam ? (DiemTanCong + DiemPhongNgu) : DiemTam;

                        if (DiemMax < DiemTong)
                        {
                            DiemMax = DiemTong;

                            ChessPoint = new Point(i, j);

                        }

                    }

                }
            }
            return ChessPoint;
        }
        long DiemTanCong_Doc(int dong, int cot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            int SoQuanTa2 = 0;
            int SoQuanDich2 = 0;
            if (dong + 1 < Cons.ChessBoard_Height && Matrix[dong + 1][cot].BackgroundImage == null)
            {
            }
            if (dong > 0 && Matrix[dong - 1][cot].BackgroundImage == null)
            {

            }
            for (int dem = 1; dem < 6 && dem + dong < Cons.ChessBoard_Height; dem++)
            {
                if (Matrix[dong + dem][cot].BackgroundImage == Player[0].Mark)
                    SoQuanTa++;
                else
                {
                    if (Matrix[dong + dem][cot].BackgroundImage == Player[1].Mark)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && dong + dem2 < Cons.ChessBoard_Height; dem2++)
                            if (Matrix[dong + dem2][cot].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                            }
                            else if (Matrix[dong + dem2][cot].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                                break;
                            }
                            else
                                break;
                        break;
                    }
                }
            }
            for (int dem = 1; dem < 6 && dong - dem >= 0; dem++)
            {
                if (Matrix[dong - dem][cot].BackgroundImage == Player[0].Mark)
                    SoQuanTa++;
                else
                {
                    if (Matrix[dong - dem][cot].BackgroundImage == Player[1].Mark)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && dong - dem2 >= 0; dem2++)
                            if (Matrix[dong - dem2][cot].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                            }
                            else if (Matrix[dong - dem2][cot].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                                break;
                            }
                            else
                                break;
                        break;
                    }
                }
            }



            if (SoQuanDich == 2)
                return 0;
            if (SoQuanDich == 0)
                DiemTong += TanCong[SoQuanTa] * 2;
            else
                DiemTong += TanCong[SoQuanTa];
            if (SoQuanDich2 == 0)
                DiemTong += TanCong[SoQuanTa2] * 2;
            else
                DiemTong += TanCong[SoQuanTa2];
            if (SoQuanTa >= SoQuanTa2)
                DiemTong -= 1;
            else
                DiemTong -= 2;
            if (SoQuanTa == 4)
                DiemTong *= 2;
            if (SoQuanTa == 0)
                DiemTong += PhongNgu[SoQuanDich] * 2;
            else
                DiemTong += PhongNgu[SoQuanDich];
            if (SoQuanTa2 == 0)
                DiemTong += PhongNgu[SoQuanDich2] * 2;
            else
                DiemTong += PhongNgu[SoQuanDich2];

            return DiemTong;
        }

        long DiemTanCong_Ngang(int dong, int cot)
        {
            long DiemTong = 0;

            int SoQuanTa = 0;
            int SoQuanDich = 0;
            int SoQuanTa2 = 0;
            int SoQuanDich2 = 0;

            if (cot + 1 < Cons.ChessBoard_Width && Matrix[dong][cot + 1].BackgroundImage == null)
            {

            }
            if (cot > 0 && Matrix[dong][ cot - 1].BackgroundImage == null)
            {

            }

            for (int dem = 1; dem < 6 && dem + cot < Cons.ChessBoard_Width; dem++)
            {
                if (Matrix[dong][cot+dem].BackgroundImage == Player[0].Mark)
                    SoQuanTa++;
                else
                {
                    if (Matrix[dong][cot+dem].BackgroundImage == Player[1].Mark)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && cot + dem2 < Cons.ChessBoard_Width; dem2++)
                            if (Matrix[dong][ cot + dem2].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                            }
                            else if (Matrix[dong][cot + dem2].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                                break;
                            }
                            else
                                break;
                        break;
                    }
                }
                 
            }
            for (int dem = 1; dem < 6 && cot - dem >= 0; dem++)
            {
                if (Matrix[dong][cot - dem].BackgroundImage == Player[0].Mark)
                    SoQuanTa++;
                else
                {
                    if (Matrix[dong][cot - dem].BackgroundImage == Player[1].Mark)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && cot - dem2 >=0; dem2++)
                            if (Matrix[dong][cot - dem2].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                            }
                            else if (Matrix[dong][cot - dem2].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                                break;
                            }
                            else
                                break;
                        break;
                    }
                }
            }

            if (SoQuanDich == 2)
                return 0;
            if (SoQuanDich == 0)
                DiemTong += TanCong[SoQuanTa] * 2;
            else
                DiemTong += TanCong[SoQuanTa];
            if (SoQuanDich2 == 0)
                DiemTong += TanCong[SoQuanTa2] * 2;
            else
                DiemTong += TanCong[SoQuanTa2];
            if (SoQuanTa >=SoQuanTa2)
                DiemTong -= 1;
            else
                DiemTong -= 2;
            if (SoQuanTa == 4)
                DiemTong *= 2;
            if (SoQuanTa == 0)
                DiemTong += PhongNgu[SoQuanDich] * 2;
            else
                DiemTong += PhongNgu[SoQuanDich];
            if (SoQuanTa2 == 0)
                DiemTong += PhongNgu[SoQuanDich2] * 2;
            else
                DiemTong += PhongNgu[SoQuanDich2];

            return DiemTong;
        }

        long DiemTanCong_Cheo(int dong, int cot)
        {
            long DiemTong = 0;

            int SoQuanTa = 0;
            int SoQuanDich = 0;
            int SoQuanTa2 = 0;
            int SoQuanDich2 = 0;

            if (dong + 1 < Cons.ChessBoard_Height && cot + 1 < Cons.ChessBoard_Width && Matrix[dong + 1][ cot + 1].BackgroundImage == null) { }
            if (dong > 0 && cot > 0 && Matrix[dong - 1][cot - 1].BackgroundImage == null) { }
            for (int dem = 1; dem < 6 && dem + dong < Cons.ChessBoard_Height && cot + dem <Cons.ChessBoard_Width; dem++)
            {
                if (Matrix[dong + dem][cot + dem].BackgroundImage == Player[0].Mark)
                    SoQuanTa++;
                else
                {
                    if (Matrix[dong + dem][cot + dem].BackgroundImage == Player[1].Mark)
                    { 
                        
                        SoQuanDich++;
                         break;
                    }
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && cot + dem2 < Cons.ChessBoard_Width && dong + dem2 < Cons.ChessBoard_Height; dem2++)
                            if (Matrix[dong + dem2][ cot + dem2].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                            }
                            else if (Matrix[dong + dem2][ cot + dem2].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                                break;
                            }
                            else
                                break;
                        break;
                    }
                }
            }
            for (int dem = 1; dem < 6 && dong - dem >= 0 && cot - dem >=0; dem++)
            {
                if (Matrix[dong - dem][cot - dem].BackgroundImage == Player[0].Mark)
                    SoQuanTa++;
                else
                {
                    if (Matrix[dong - dem][cot - dem].BackgroundImage == Player[1].Mark)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && cot - dem2 >= 0 && dong - dem2 >= 0; dem2++)
                            if (Matrix[dong - dem2][cot- dem2].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                            }
                            else if (Matrix[dong - dem2][cot - dem2].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                                break;
                            }
                            else
                                break;
                        break;
                    }
                }
            }

            if (SoQuanDich == 2)
                return 0;
            if (SoQuanDich == 0)
                DiemTong += TanCong[SoQuanTa] * 2;
            else
                DiemTong += TanCong[SoQuanTa];
            if (SoQuanDich2 == 0)
                DiemTong += TanCong[SoQuanTa2] * 2;
            else
                DiemTong += TanCong[SoQuanTa2];
            if (SoQuanTa >= SoQuanTa2)
                DiemTong -= 1;
            else
                DiemTong -= 2;

            if (SoQuanTa == 4)
                DiemTong *= 2;
            if (SoQuanTa == 0)
                DiemTong += PhongNgu[SoQuanDich] * 2;
            else
                DiemTong += PhongNgu[SoQuanDich];
            if (SoQuanTa2 == 0)
                DiemTong += PhongNgu[SoQuanDich2] * 2;
            else
                DiemTong += PhongNgu[SoQuanDich2];

            return DiemTong;
        }
        long DiemTanCong_CheoNguoc(int dong, int cot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            int SoQuanTa2 = 0;
            int SoQuanDich2 = 0;
            if (dong > 0 && cot + 1 < Cons.ChessBoard_Width && Matrix[dong - 1][cot + 1].BackgroundImage == null) { }
            if (dong + 1 < Cons.ChessBoard_Height && cot > 0 && Matrix[dong + 1][cot - 1].BackgroundImage == null) { }
            for (int dem = 1; dem < 6 && dem + dong < Cons.ChessBoard_Height && cot - dem >= 0; dem++)
            {
                if (Matrix[dong + dem][cot - dem].BackgroundImage == Player[0].Mark)
                    SoQuanTa++;
                else
                {
                    if (Matrix[dong + dem][cot - dem].BackgroundImage == Player[1].Mark)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && cot - dem2 >=0 && dong+ dem2 <Cons.ChessBoard_Height; dem2++)
                            if (Matrix[dong + dem2][ cot - dem2].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                            }
                            else if (Matrix[dong + dem2][cot - dem2].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                                break;
                            }
                            else
                                break;
                        break;
                    }
                }
            }
            for (int dem = 1; dem < 6 && dong - dem >= 0 && cot + dem < Cons.ChessBoard_Width; dem++)
            {
                if (Matrix[dong - dem][cot + dem].BackgroundImage == Player[0].Mark)
                    SoQuanTa++;
                else
                {
                    if (Matrix[dong - dem][cot + dem].BackgroundImage == Player[1].Mark)
                    {
                        SoQuanDich++;
                        break;
                    }
                    else
                    {
                        for (int dem2 = 1; dem2 < 6 && cot + dem2 < Cons.ChessBoard_Width && dong - dem2 >=0; dem2++)
                            if (Matrix[dong- dem2][ cot + dem2].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                            }
                            else if (Matrix[dong - dem2][cot + dem2].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                                break;
                            }
                            else
                                break;
                        break;
                    }
                }
            }

            if (SoQuanDich== 2)
                return 0;
            if (SoQuanDich == 0)
                DiemTong += TanCong[SoQuanTa] * 2;
            else
                DiemTong += TanCong[SoQuanTa];
            if (SoQuanDich2 == 0)
                DiemTong += TanCong[SoQuanTa2] * 2;
            else
                DiemTong += TanCong[SoQuanTa2];
            if (SoQuanTa >= SoQuanTa2)
                DiemTong -= 1;
            else
                DiemTong -= 2;
            if (SoQuanTa == 4)
                DiemTong *= 2;
            if (SoQuanTa == 0)
                DiemTong += PhongNgu[SoQuanDich] * 2;
            else
                DiemTong += PhongNgu[SoQuanDich];
            if (SoQuanTa2 == 0)
                DiemTong += PhongNgu[SoQuanDich2] * 2;
            else
                DiemTong += PhongNgu[SoQuanDich2];

            return DiemTong;
        }
        long DiemPhongNgu_Doc(int dong, int cot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            int SoQuanTa2 = 0;
            int SoQuanDich2 = 0;
            for (int dem = 1; dem < 6 && dem + dong < Cons.ChessBoard_Height; dem++)
            {
                if (Matrix[dong + dem][cot].BackgroundImage == Player[0].Mark)
                {
                    SoQuanTa++;
                    break;
                }
                else
                {
                    if (Matrix[dong + dem][cot].BackgroundImage == Player[1].Mark)
                        SoQuanDich++;
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && dong + dem2 < Cons.ChessBoard_Height; dem2++)
                            if (Matrix[dong + dem2][ cot].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                                break;
                            }
                            else if (Matrix[dong + dem2][cot].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                            }
                            else
                                break;
                        break;
                    }
                }
            }
            for (int dem = 1; dem < 6 && dong - dem >= 0; dem++)
            {
                if (Matrix[dong - dem][cot].BackgroundImage == Player[0].Mark)
                {
                    SoQuanTa++;
                    break;
                }
                else
                {
                    if (Matrix[dong - dem][cot].BackgroundImage == Player[1].Mark)
                        SoQuanDich++;
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && dong - dem2 >= 0; dem2++)
                            if (Matrix[dong - dem2][cot].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                                break;
                            }
                            else if (Matrix[dong - dem2][cot].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                            }
                            else
                                break;
                        break;
                    }
                }
            }
            if (SoQuanTa == 2)
                return 0;
            if (SoQuanTa == 0)
                DiemTong += PhongNgu[SoQuanDich] * 2;
            else
                DiemTong += PhongNgu[SoQuanDich];
            if (SoQuanDich >= SoQuanDich2)
                DiemTong -= 1;
            else
                DiemTong -= 2;
            if (SoQuanDich == 4)
                DiemTong *= 2;

            return DiemTong;
        }
        long DiemPhongNgu_Ngang(int dong, int cot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            int SoQuanTa2 = 0;
            int SoQuanDich2 = 0;
            for (int dem = 1; dem < 6 && dem + cot < Cons.ChessBoard_Width; dem++)
            {
                if (Matrix[dong][cot+dem].BackgroundImage == Player[0].Mark)
                {
                    SoQuanTa++;
                    break;
                }
                else
                {
                    if (Matrix[dong][cot + dem].BackgroundImage == Player[1].Mark)
                        SoQuanDich++;
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && cot + dem2 < Cons.ChessBoard_Width; dem2++)
                            if (Matrix[dong][ cot + dem2].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                                break;
                            }
                            else if (Matrix[dong][ cot + dem2].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                            }
                            else
                                break;
                        break;
                    }
                }
            }
            for (int dem = 1; dem < 6 && cot - dem >= 0; dem++)
            {
                if (Matrix[dong][cot - dem].BackgroundImage == Player[0].Mark)
                {
                    SoQuanTa++;
                    break;
                }
                else
                {
                    if (Matrix[dong][cot - dem].BackgroundImage == Player[1].Mark)
                        SoQuanDich++;
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && cot - dem2 >= 0; dem2++)
                            if (Matrix[dong][ cot - dem2].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                                break;
                            }
                            else if (Matrix[dong][cot - dem2].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                            }
                            else break;
                        break;
                    }
                }
            }
            if (SoQuanTa == 2)
                return 0;
            if (SoQuanTa == 0)
                DiemTong += PhongNgu[SoQuanDich] * 2;
            else
                DiemTong += PhongNgu[SoQuanDich];
            if (SoQuanDich >= SoQuanDich2)
                DiemTong -= 1;
            else
                DiemTong -= 2;
            if (SoQuanDich == 4)
                DiemTong *= 2;

            return DiemTong;
        }
        long DiemPhongNgu_Cheo(int dong, int cot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            int SoQuanTa2 = 0;
            int SoQuanDich2 = 0;

            for (int dem = 1; dem < 6 && dem + dong < Cons.ChessBoard_Height && cot + dem < Cons.ChessBoard_Width; dem++)
            {
                if (Matrix[dong + dem][cot + dem].BackgroundImage == Player[0].Mark)
                {
                    SoQuanTa++;
                    break;
                }
                else
                {
                    if (Matrix[dong + dem][cot + dem].BackgroundImage == Player[1].Mark)
                        SoQuanDich++;
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && dong + dem2 < Cons.ChessBoard_Height && cot + dem2 < Cons.ChessBoard_Width; dem2++)
                            if (Matrix[dong + dem2][cot + dem2].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                                break;
                            }
                            else if (Matrix[dong + dem2][cot + dem2].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                            }
                            else
                                break;
                        break;
                    }

                }
            }
            for (int dem = 1; dem < 6 && dong - dem >= 0 && cot - dem >= 0; dem++)
            {
                if (Matrix[dong - dem][cot - dem].BackgroundImage == Player[0].Mark)
                {
                    SoQuanTa++;
                    break;
                }
                else
                {
                    if (Matrix[dong - dem][cot - dem].BackgroundImage == Player[1].Mark)
                        SoQuanDich++;
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && cot - dem2 >= 0 && dong - dem2 >= 0; dem2++)
                            if (Matrix[dong - dem2][cot - dem2].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                                break;
                            }
                            else if (Matrix[dong - dem2][cot - dem2].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                            }
                            else
                                break;
                        break;
                    }

                }
            }
            if (SoQuanTa == 2)
                return 0;
            if (SoQuanTa == 0)
                DiemTong += PhongNgu[SoQuanDich] * 2;
            else
                DiemTong += PhongNgu[SoQuanDich];
            if (SoQuanDich >= SoQuanDich2)
                DiemTong -= 1;
            else
                DiemTong -= 2;
            if (SoQuanDich == 4)
                DiemTong *= 2;

            return DiemTong;
        }
        long DiemPhongNgu_CheoNguoc(int dong, int cot)
        {
            long DiemTong = 0;
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            int SoQuanTa2 = 0;
            int SoQuanDich2 = 0;
            for (int dem = 1; dem < 6 && dem + dong < Cons.ChessBoard_Height && cot - dem >= 0; dem++)
            {
                if (Matrix[dong+ dem][cot - dem].BackgroundImage == Player[0].Mark)
                {
                    SoQuanTa++;
                    break;
                }
                else
                {
                    if (Matrix[dong+ dem][cot - dem].BackgroundImage == Player[1].Mark)
                        SoQuanDich++;
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && dong + dem2 <Cons.ChessBoard_Height && cot - dem2 >=0; dem2++)
                            if (Matrix[dong + dem2][ cot - dem2].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                                break;
                            }
                            else if (Matrix[dong + dem2][cot +- dem2].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                            }
                            else
                                break;
                        break;
                    }

                }
            }
            for (int dem = 1; dem < 6 && dong - dem >= 0 && cot + dem < Cons.ChessBoard_Width; dem++)
            {
                if (Matrix[dong - dem][cot+ dem].BackgroundImage == Player[0].Mark)
                {
                    SoQuanTa++;
                    break;
                }
                else
                {
                    if (Matrix[dong- dem][cot+ dem].BackgroundImage == Player[1].Mark)
                        SoQuanDich++;
                    else
                    {
                        for (int dem2 = 2; dem2 < 6 && dong - dem2 >=0 && cot + dem2 <Cons.ChessBoard_Width; dem2++)
                            if (Matrix[dong - dem2][ cot + dem2].BackgroundImage == Player[0].Mark)
                            {
                                SoQuanTa2++;
                                break;
                            }
                            else if (Matrix[dong - dem2][cot + dem2].BackgroundImage == Player[1].Mark)
                            {
                                SoQuanDich2++;
                            }
                            else
                                break;
                        break;
                    }
                }
            }

            if (SoQuanTa == 2)
                return 0;
            if (SoQuanTa == 0)
                DiemTong += PhongNgu[SoQuanDich] * 2;
            else
                DiemTong += PhongNgu[SoQuanDich];
            if (SoQuanDich >= SoQuanDich2)
                DiemTong -= 1;
            else
                DiemTong -= 2;
            if (SoQuanDich == 4)
                DiemTong *= 2;
            return DiemTong;
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            DrawChessBoard();
            pnlChessBoard.Enabled = true;
            btnStart.Enabled = false;
            btnPlayervsPlayer.Enabled = true;
            btnPlayervsCOM.Enabled = true;
            btnUndo.Enabled = true;
            btnRedo.Enabled = true;
            undoToolStripMenuItem.Enabled = true;
            redoToolStripMenuItem.Enabled = true;

            if (P2 == null)
                PlayervsCom(P1);
            else if (P1 != "" && P2 != "")
                PlayervsPlayer(P1,P2);

            P1 = null;
            P2 = null;      
        }

        private void Mark(Button btn) {
            btn.BackgroundImage = Player[current].Mark;
            Sound();
        }    
        private void btnPlayervsPlayer_Click(object sender, EventArgs e)
        {
            NewPvsP();
        }
        private void NewPvsP()
        {
            if (MessageBox.Show("Do you want to start a new game ?", "Game Caro", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                prcbCoolDown.Value = 0;
                tmCoolDown.Stop();
                if (P1vsP2 == null)
                {
                    P1vsP2 = new frmPvsP();
                    P1vsP2.PlayGame = this;
                }
                P1vsP2.Show();
                this.Hide();
                pnlChessBoard.Enabled = false;
                btnStart.Enabled = true;
                btnPlayervsCOM.Enabled = false;
                btnPlayervsPlayer.Enabled = false;
                btnRedo.Enabled = false;
                btnUndo.Enabled = false;
            }
        }
        private void NewPvsC()
        {
            if (MessageBox.Show("Do you want to start a new game ?", "Game Caro", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                prcbCoolDown.Value = 0;
                tmCoolDown.Stop();
                if (PvsCOM == null)
                {
                    PvsCOM = new frmPvsC();
                    PvsCOM.PlayGame = this;
                }
                PvsCOM.Show();
                this.Hide();               
                pnlChessBoard.Enabled = false;
                btnStart.Enabled = true;
                btnPlayervsCOM.Enabled = false;
                btnPlayervsPlayer.Enabled = false;
                btnRedo.Enabled = false;
                btnUndo.Enabled = false;
            }

        }
        private void btnPlayervsCOM_Click(object sender, EventArgs e)
        {
            NewPvsC();
        }

        private bool isEndGame(Button btn)
        {

            return isEndHorizontal(btn) || isEndVertical(btn) || isEndPrimaryCross(btn) || isEndPrimary(btn);
        }
        private void EndGame()
        {
            tmCoolDown.Stop();
            pnlChessBoard.Enabled = false;
            btnUndo.Enabled = false;
            btnRedo.Enabled = false;
            undoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Enabled = false;
            if(current == 0)
            {
                MessageBox.Show("Chúc mừng " + Player[1].Name + " đã chiến thắng " + Player[current].Name + " !!");
            }
            else    
                if (current == 1)
                {
                    MessageBox.Show("Chúc mừng " + Player[0].Name + " đã chiến thắng "+ Player[current].Name + " !!");
                }
        }
        private Point GetChessPoint(Button btn){
            
            int vertical = Convert.ToInt32(btn.Tag);
            int horizontal = Matrix[vertical].IndexOf(btn);

            Point point = new Point(horizontal,vertical);

            return point;
        }
        private bool isEndHorizontal(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countLeft = 0;

            for(int i=point.X; i >= 0; i--)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countLeft++;
                }
                else break;
            }

            int countRight = 0;
            for (int i = point.X+1; i <Cons.ChessBoard_Width; i++)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countRight++;
                }
                else break;
            }


            return countLeft+countRight>=5;
        }
        private bool isEndVertical(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTop = 0;

            for (int i = point.Y; i >=0; i--)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else break;
            }

            int countBottom = 0;
            for (int i = point.Y + 1; i < Cons.ChessBoard_Height; i++)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else break;
            }


            return countTop + countBottom >= 5;
        }
        private bool isEndPrimaryCross(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTop = 0;

            for (int i = 0; i <=point.X; i++)
            {
                if (point.X - i < 0 || point.Y - i < 0) break;

                if (Matrix[point.Y-i][point.X-i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else break;
            }

            int countBottom = 0;
            for (int i = 1; i <= Cons.ChessBoard_Width - point.X; i++)
            {
                if (point.Y + i >= Cons.ChessBoard_Height || point.X + i >= Cons.ChessBoard_Width) break;

                if (Matrix[point.Y + i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else break;
            }
            return countTop + countBottom >= 5;
        }
        private bool isEndPrimary(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTop = 0;

            for (int i = 0; i <= point.X; i++)
            {
                if (point.X + i >Cons.ChessBoard_Width || point.Y - i < 0) break;

                if (Matrix[point.Y - i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else break;
            }
            int countBottom = 0;
            for (int i = 1; i <= Cons.ChessBoard_Width - point.X; i++)
            {
                if (point.Y + i >= Cons.ChessBoard_Height || point.X - i <0) break;

                if (Matrix[point.Y + i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else break;
            }
            return countTop + countBottom >= 5;
        }
        private void tmCoolDown_Tick(object sender, EventArgs e)
        {
            prcbCoolDown.PerformStep();
            if (prcbCoolDown.Value >= prcbCoolDown.Maximum)
            {
                EndGame(); 
            }
        }
        private void playerVsPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewPvsP();
        }
        private void playerVsCOMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewPvsC();
        }
        private void btnUndo_Click(object sender, EventArgs e)
        {
            Undo();
        }
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Undo();
        }
        private void btnRedo_Click(object sender, EventArgs e)
        {
            Redo();
        }
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Redo();
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Caro Chess : Version 1.0");
        }
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Quit();
        }
    }
}
