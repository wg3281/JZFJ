using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Drawing;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using BusinessLogic.Print;
using Microsoft.Win32.SafeHandles;

namespace PosPrint
{

    /// <summary>
    /// ������ӡ��������
    /// </summary>
    public class LPTPrinter
    {

        private PrintDocument ThePrintDocument;
        private Font titleFont;
        private Font theFont;
        private Color FontColor;
        private float CurrentY;
        static int PageNumber;
        private int PageWidth;
        private int PageHeight;
        private int LeftMargin;
        private int TopMargin;
        private int RightMargin;
        private int BottomMargin;
        private ArrayList textList = new ArrayList();
        private int currentIndex = 0;
        private float fontHeight;
        private PrintRows printRows;

        public LPTPrinter()
        {
            
        }

        internal LPTPrinter(PrintDocument _thePrintDocument, Color _FontColor)
        {
            ThePrintDocument = _thePrintDocument;
            //theFont = _theFont;
            //titleFont = _titleFont;
            FontColor = _FontColor;
            PageNumber = 0;
            
            
            if (!ThePrintDocument.DefaultPageSettings.Landscape)
            {                
                PageWidth = ThePrintDocument.DefaultPageSettings.PaperSize.Width;
                PageHeight = ThePrintDocument.DefaultPageSettings.PaperSize.Height;
            }
            else
            {
                PageHeight = ThePrintDocument.DefaultPageSettings.PaperSize.Width;
                PageWidth = ThePrintDocument.DefaultPageSettings.PaperSize.Height;
            }
            LeftMargin = 0;
            TopMargin = 10;
            RightMargin = ThePrintDocument.DefaultPageSettings.Margins.Right;
            BottomMargin = ThePrintDocument.DefaultPageSettings.Margins.Bottom;
        }

        internal LPTPrinter(PrintDocument _thePrintDocument, Font _theFont, Color _FontColor)
        {
            ThePrintDocument = _thePrintDocument;
            theFont = _theFont;
            //titleFont = _titleFont;
            FontColor = _FontColor;
            PageNumber = 0;
            //theOrder = _order;
            if (!ThePrintDocument.DefaultPageSettings.Landscape)
            {
                PageWidth = ThePrintDocument.DefaultPageSettings.PaperSize.Width;
                PageHeight = ThePrintDocument.DefaultPageSettings.PaperSize.Height;
            }
            else
            {
                PageHeight = ThePrintDocument.DefaultPageSettings.PaperSize.Width;
                PageWidth = ThePrintDocument.DefaultPageSettings.PaperSize.Height;
            }
            LeftMargin = ThePrintDocument.DefaultPageSettings.Margins.Left;
            TopMargin = ThePrintDocument.DefaultPageSettings.Margins.Top;
            RightMargin = ThePrintDocument.DefaultPageSettings.Margins.Right;
            BottomMargin = ThePrintDocument.DefaultPageSettings.Margins.Bottom;
        }



        /// <summary>
        /// Draws the document.
        /// </summary>
        /// <param name="g">The g.</param>
        /// <returns></returns>
        public bool DrawDocument(Graphics g)
        {
            try
            {
                //DrawHeader(g);
                bool bContinue = DrawItems(g);
                //DrawFloor(g);
                g.Dispose();
                return bContinue;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ʧ��" + ex.Message.ToString(), " - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                g.Dispose();
                return false;
            }
        }

        /// <summary>
        /// Draws the header.
        /// </summary>
        /// <param name="g">The Graphics</param>
        public void DrawHeader(Graphics g)
        {
            CurrentY = (float)TopMargin + MillimeterToInch(Invoice.BlankHeight_M);
            PageNumber++;
            StringFormat TitleFormat = new StringFormat();
            TitleFormat.Trimming = StringTrimming.Word;
            TitleFormat.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit | StringFormatFlags.NoClip;
            TitleFormat.Alignment = StringAlignment.Near;
            RectangleF TitleRectangle = new RectangleF((float)LeftMargin, CurrentY, (float)PageWidth - (float)RightMargin - (float)LeftMargin, g.MeasureString("�շѵ�", titleFont).Height);
            RectangleF TitleRectangle1 = new RectangleF((float)LeftMargin + 200, CurrentY, 50, g.MeasureString("����", theFont).Height);
            g.DrawString(Invoice.Titel[0].ToString(), titleFont, new SolidBrush(FontColor), TitleRectangle, TitleFormat);
            g.DrawString(Invoice.Titel[1].ToString(), theFont, new SolidBrush(FontColor), TitleRectangle1, TitleFormat);
            CurrentY += g.MeasureString(Invoice.Titel[0].ToString(), titleFont).Height;
        }

        /// <summary>
        /// Draws the items.
        /// </summary>
        /// <param name="g">The Graphics</param>
        /// <returns></returns>
        public bool DrawItems(Graphics g)
        {
            StringFormat TextFormat = new StringFormat();
            TextFormat.Trimming = StringTrimming.Word;
            TextFormat.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit | StringFormatFlags.NoClip;
            TextFormat.Alignment = StringAlignment.Near;


            printRows = PrintRows.GetPrintRows();
            for (int i = currentIndex; i < Invoice.Details.Count; i++)
            {
                string var = Invoice.Details[i].DetailName;
                if (var.Contains("��ַ") && var.Length > 13)
                {
                    string address1 = "";
                    string address2 = "";
                    address1 = var.Substring(0, 13);
                    address2 = var.Substring(13,var.Length-address1.Length);
                    RectangleF TextRectangle = new RectangleF((float)LeftMargin + Invoice.Details[i].X, printRows[Invoice.Details[i].Y] + 2, (float)PageWidth - (float)RightMargin - (float)LeftMargin, g.MeasureString("��ǩ", new Font(Invoice.Details[i].Font, Invoice.Details[i].FontSize)).Height);
                    g.DrawString(address1, new Font(Invoice.Details[i].Font, Invoice.Details[i].FontSize), new SolidBrush(FontColor), TextRectangle, TextFormat);

                    TextRectangle = new RectangleF((float)LeftMargin + Invoice.Details[i].X + 45, printRows[Invoice.Details[i].Y + 1] + 2, (float)PageWidth - (float)RightMargin - (float)LeftMargin, g.MeasureString("��ǩ", new Font(Invoice.Details[i].Font, Invoice.Details[i].FontSize)).Height);
                    g.DrawString(address2, new Font(Invoice.Details[i].Font, Invoice.Details[i].FontSize), new SolidBrush(FontColor), TextRectangle, TextFormat);

                }
                else
                {
                    RectangleF TextRectangle = new RectangleF((float)LeftMargin + Invoice.Details[i].X, printRows[Invoice.Details[i].Y] + 2, (float)PageWidth - (float)RightMargin - (float)LeftMargin, g.MeasureString("��ǩ", new Font(Invoice.Details[i].Font, Invoice.Details[i].FontSize)).Height);
                    g.DrawString(var, new Font(Invoice.Details[i].Font, Invoice.Details[i].FontSize), new SolidBrush(FontColor), TextRectangle, TextFormat);

                }
                

                //fontHeight = g.MeasureString(var, new Font(Invoice.Details[i].Font, 13)).Height;
                //CurrentY = CurrentY + fontHeight;
                //�����Ƿ������˷�ҳ��ӡ����
                if (Invoice.IsPageturn)
                {
                    //ת����ӡ������ĺ���ΪӢ��
                    float itemheight = (MillimeterToInch(Invoice.ItemHeight_M));

                    //��ӡ�����ѳ������õĸ߶�
                    if (CurrentY > itemheight)
                    {
                        //���ҳ����߶�ʱ����ӡ���ݻ�δ����
                        if (i < Invoice.Details.Count - 1)
                        {
                            //���÷�ҳ�����һ����ʼ
                            currentIndex = i + 1;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                
            }
            return false;
        }



        /// <summary>
        /// Draws the floor.
        /// </summary>
        /// <param name="g">The Graphics.</param>
        public void DrawFloor(Graphics g)
        {
            StringFormat FloorFormat = new StringFormat();
            FloorFormat.Trimming = StringTrimming.Word;
            FloorFormat.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit | StringFormatFlags.NoClip;
            FloorFormat.Alignment = StringAlignment.Center;           
            
            CurrentY = MillimeterToInch(Invoice.ItemHeight_M) + fontHeight * 2;
            
            RectangleF FloorRectangle;
            for (int i = 0; i < Invoice.Floot.Count; i++)
            {
                string var = Invoice.Floot[i].ToString();
                FloorRectangle = new RectangleF((float)LeftMargin, CurrentY, (float)PageWidth - (float)RightMargin - (float)LeftMargin, g.MeasureString("�շѵ�", titleFont).Height);
                g.DrawString(var, theFont, new SolidBrush(FontColor), FloorRectangle, FloorFormat);
                CurrentY = CurrentY + fontHeight;
            }
            FloorRectangle = new RectangleF((float)LeftMargin, CurrentY, (float)PageWidth - (float)RightMargin - (float)LeftMargin, g.MeasureString("�շѵ�", titleFont).Height);
            CurrentY += fontHeight;
            FloorRectangle = new RectangleF((float)LeftMargin, CurrentY, (float)PageWidth - (float)RightMargin - (float)LeftMargin, g.MeasureString("�շѵ�", titleFont).Height);
            g.DrawString("_", titleFont, new SolidBrush(Color.White), FloorRectangle, FloorFormat);
            CurrentY += fontHeight;
        }


        /// <summary>
        /// Millimeters to inch.
        /// </summary>
        /// <param name="millimeter">The millimeter.</param>
        /// <returns></returns>
        private float MillimeterToInch(float millimeter)
        {
            return float.Parse((millimeter / 0.256).ToString());
        }




        private string firstpageHeader;

        /// <summary>
        /// Gets or sets the page header.
        /// </summary>
        /// <value>The page header.</value>
        public string FirstPageHeader
        {
            get { return firstpageHeader; }
            set { firstpageHeader = value; }
        }

        private string firstpageFooter;


        /// <summary>
        /// Gets or sets the first page footer.
        /// </summary>
        /// <value>The first page footer.</value>
        public string FirstPageFooter
        {
            get { return firstpageFooter; }
            set { firstpageFooter = value; }
        }

        private string secondpageFooter;

        /// <summary>
        /// Gets or sets the second page footer.
        /// </summary>
        /// <value>The second page footer.</value>
        public string SecondPageFooter
        {
            get { return secondpageFooter; }
            set { secondpageFooter = value; }
        }

        private string thirdpageFooter;

        /// <summary>
        /// Gets or sets the third page footer.
        /// </summary>
        /// <value>The third page footer.</value>
        public string ThirdPageFooter
        {
            get { return thirdpageFooter; }
            set { thirdpageFooter = value; }
        }


    }


    /// <summary>
    /// ֱ��ʹ��API��ӡ
    /// </summary>
    public class APIPrinter
    {
        const int OPEN_EXISTING = 3;
        string prnPort = "LPT1";
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(string lpFileName,
        int dwDesiredAccess,
        int dwShareMode,
        int lpSecurityAttributes,
        int dwCreationDisposition,
        int dwFlagsAndAttributes,
        int hTemplateFile);

        /// <summary>
        /// Initializes a new instance of the <see cref="APIPrinter"/> class.
        /// </summary>
        public APIPrinter()
        {
            //   
            //   TODO:   �ڴ˴����ӹ��캯���߼�   
            //   
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="APIPrinter"/> class.
        /// </summary>
        /// <param name="prnPort">The PRN port.</param>
        public APIPrinter(string prnPort)
        {
            this.prnPort = prnPort;//��ӡ���˿�   
        }

        /// <summary>
        /// Opens the cash box.
        /// </summary>
        /// <returns></returns>
        public string OpenCashBox()
        {
            try
            {

                IntPtr iHandle = CreateFile(prnPort, 0x40000000, 0, 0, OPEN_EXISTING, 0, 0);
                if (iHandle.ToInt32() == -1)
                {
                    return "û�����Ӵ�ӡ�����ߴ�ӡ���˿ڲ���LPT1";
                }

                SafeFileHandle safeFileHandle = new SafeFileHandle(iHandle, true);
                FileStream fileStream = new FileStream(safeFileHandle, FileAccess.ReadWrite);

                StreamWriter sw = new StreamWriter(fileStream, System.Text.Encoding.Default); //д����                   
                //��Ǯ��   
                char[] Instruction = {(char) (27), 'p', (char) (0), (char) (25), (char) (255)};
                sw.Write(Instruction);
                sw.Close();
                fileStream.Close();

                return "";
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Prints the blankline.
        /// </summary>
        /// <returns></returns>
        public string PrintBlankline()
        {

            IntPtr iHandle = CreateFile(prnPort, 0x40000000, 0, 0, OPEN_EXISTING, 0, 0);
            if (iHandle.ToInt32() == -1)
            {
                return "û�����Ӵ�ӡ�����ߴ�ӡ���˿ڲ���LPT1";
            }
            else
            {                
                FileStream fs = new FileStream(iHandle, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);   //д����   
                sw.WriteLine("");                
                sw.Close();
                fs.Close();
                return "";
            }
        }


        

       
    }
}  

    
