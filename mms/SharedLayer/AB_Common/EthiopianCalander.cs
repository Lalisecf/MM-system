using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLayer.AB_Common
{
    public class EthiopianCalendar
    {
        // Fields
        public const int JD_EPOCH_OFFSET_AMETE_ALEM = -285019;
        public const int JD_EPOCH_OFFSET_AMETE_MIHRET = 0x1a4dd0;
        public const int JD_EPOCH_OFFSET_AMETE_COPTIC = 0x1bd799;
        public const int JD_EPOCH_OFFSET_AMETE_GREGORIAN = 0x1a4452;
        public const int JD_EPOCH_OFFSET_AMETE_UNSET = -1;
        private int jdoffset;
        private int year;
        private int month;
        private int day;
        private bool dateIsUnset;
        private static int nMonths = 12;
        private static int[] monthDays = new int[] { 0, 0x1f, 0x1c, 0x1f, 30, 0x1f, 30, 0x1f, 0x1f, 30, 0x1f, 30, 0x1f };

        // Methods
        public EthiopianCalendar()
        {
            this.jdoffset = -1;
            this.year = -1;
            this.month = -1;
            this.day = -1;
            this.dateIsUnset = true;
        }

        public EthiopianCalendar(int year, int month, int day)
        {
            this.jdoffset = -1;
            this.year = -1;
            this.month = -1;
            this.day = -1;
            this.dateIsUnset = true;
            this.set(year, month, day);
        }

        public EthiopianCalendar(int year, int month, int day, int era)
        {
            this.jdoffset = -1;
            this.year = -1;
            this.month = -1;
            this.day = -1;
            this.dateIsUnset = true;
            this.set(year, month, day, era);
        }

        public string ConvertethiopicToGregorian(int year, int month, int day)
        {
            if (!this.isEraSet())
            {
                if (year <= 0)
                {
                    this.setEra(-285019);
                }
                else
                {
                    this.setEra(0x1a4dd0);
                }
            }
            int j = this.ethiopicToJDN(year, month, day);
            int[] numArray = this.jdnToGregorian(j);
            string[] strArray = new string[] { numArray[2].ToString("00"), "/", numArray[1].ToString("00"), "/", numArray[0].ToString("0000") };
            return string.Concat(strArray);
        }

        public string ConvertgregorianToEthiopic(int year, int month, int day)
        {
            int jdn = this.gregorianToJDN(year, month, day);
            int[] numArray = this.jdnToEthiopic(jdn, this.guessEraFromJDN(jdn));
            string[] strArray = new string[] { numArray[2].ToString("00"), "/", numArray[1].ToString("00"), "/", numArray[0].ToString("0000") };
            return string.Concat(strArray);
        }

        private int ethCopticToJDN(int year, int month, int day, int era)
        {
            return (((((0x1a4f3d + (0x16d * (year - 1))) + this.quotient((long)year, 4L)) + (30 * month)) + day) - 0x1f);
        }

        public int[] ethiopicToGregorian()
        {
            return this.ethiopicToGregorian(this.year, this.month, this.day);
        }

        public string ethiopicToGregorian(bool format)
        {
            string str;
            string[] strArray;
            int[] numArray = this.ethiopicToGregorian();
            if (format)
            {
                strArray = new string[] { numArray[2].ToString("00"), "/", numArray[1].ToString("00"), "/", numArray[0].ToString("0000") };
                str = string.Concat(strArray);
            }
            else
            {
                strArray = new string[] { numArray[2].ToString(), "/", numArray[1].ToString(), "/", numArray[0].ToString() };
                str = string.Concat(strArray);
            }
            return str;
        }

        public int[] ethiopicToGregorian(int era)
        {
            return this.ethiopicToGregorian(this.year, this.month, this.day, era);
        }

        public int[] ethiopicToGregorian(int year, int month, int day)
        {
            if (!this.isEraSet())
            {
                if (year <= 0)
                {
                    this.setEra(-285019);
                }
                else
                {
                    this.setEra(0x1a4dd0);
                }
            }
            int j = this.ethiopicToJDN(year, month, day);
            return this.jdnToGregorian(j);
        }

        public int[] ethiopicToGregorian(int year, int month, int day, int era)
        {
            this.setEra(era);
            int[] numArray = this.ethiopicToGregorian(year, month, day);
            this.unsetEra();
            return numArray;
        }

        public int ethiopicToJDN()
        {
            return this.ethiopicToJDN(this.year, this.month, this.day);
        }

        public int ethiopicToJDN(int year, int month, int day)
        {
            return this.ethiopicToJDN(year, month, day, 0x1a4dd0);
        }

        public int ethiopicToJDN(int year, int month, int day, int era)
        {
            return this.ethCopticToJDN(year, month, day, era);
        }

        public int[] getDate()
        {
            return new int[] { this.year, this.month, this.day, this.jdoffset };
        }

        public int getDay()
        {
            return this.day;
        }

        public int getEra()
        {
            return this.jdoffset;
        }

        public int getMonth()
        {
            return this.month;
        }

        public string GetStrDate(int DayNo, int Mth, int Yr)
        {
            string[] strArray = new string[] { DayNo.ToString("00"), "/", Mth.ToString("00"), "/", Yr.ToString() };
            return string.Concat(strArray);
        }

        public int getYear()
        {
            return this.year;
        }

        public int[] gregorianToEthiopic()
        {
            return this.gregorianToEthiopic(this.year, this.month, this.day);
        }

        public string gregorianToEthiopic(bool format)
        {
            string str;
            string[] strArray;
            int[] numArray = this.gregorianToEthiopic();
            if (format)
            {
                strArray = new string[] { numArray[2].ToString("00"), "/", numArray[1].ToString("00"), "/", numArray[0].ToString("0000") };
                str = string.Concat(strArray);
            }
            else
            {
                strArray = new string[] { numArray[2].ToString(), "/", numArray[1].ToString(), "/", numArray[0].ToString() };
                str = string.Concat(strArray);
            }
            return str;
        }

        public int[] gregorianToEthiopic(int year, int month, int day)
        {
            int jdn = this.gregorianToJDN(year, month, day);
            return this.jdnToEthiopic(jdn, this.guessEraFromJDN(jdn));
        }

        public int gregorianToJDN(int year, int month, int day)
        {
            int num = ((((this.quotient((long)year, 4L) - this.quotient((long)(year - 1), 4L)) - this.quotient((long)year, (long)100)) + this.quotient((long)(year - 1), (long)100)) + this.quotient((long)year, 400L)) - this.quotient((long)(year - 1), 400L);
            int num2 = this.quotient((long)(14 - month), (long)12);
            int num3 = ((((0x1f * num2) * (month - 1)) + ((1 - num2) * (((0x3b + num) + (30 * (month - 3))) + this.quotient((long)((3 * month) - 7), 5L)))) + day) - 1;
            return (((((0x1a4452 + (0x16d * (year - 1))) + this.quotient((long)(year - 1), 4L)) - this.quotient((long)(year - 1), (long)100)) + this.quotient((long)(year - 1), 400L)) + num3);
        }

        private int guessEraFromJDN(int jdn)
        {
            return ((jdn >= 0x1a4f3d) ? 0x1a4dd0 : -285019);
        }

        public bool isDateSet()
        {
            return !this.dateIsUnset;
        }

        public bool isEraSet()
        {
            return (-1 != this.jdoffset);
        }

        private bool isGregorianLeap(int year)
        {
            return (((year % 4) == 0) && (((year % 100) != 0) || ((year % 400) == 0)));
        }

        public int[] jdnToEthiopic(int jdn)
        {
            return (this.isEraSet() ? this.jdnToEthiopic(jdn, this.jdoffset) : this.jdnToEthiopic(jdn, this.guessEraFromJDN(jdn)));
        }

        public int[] jdnToEthiopic(int jdn, int era)
        {
            long i = this.mod((long)(jdn - era), 0x5b5L);
            long num2 = this.mod(i, 0x16dL) + (0x16d * this.quotient(i, 0x5b4L));
            return new int[] { (((4 * this.quotient((long)(jdn - era), 0x5b5L)) + this.quotient(i, 0x16dL)) - this.quotient(i, 0x5b4L)), (this.quotient(num2, (long)30) + 1), (this.mod(num2, (long)30) + 1) };
        }

        public int[] jdnToGregorian(int j)
        {
            int num1;
            int num = this.mod((long)(j - 0x1a4452), 0xb2575L);
            int num2 = this.mod((long)(j - 0x1a4452), 0x23ab1L);
            int num3 = this.mod((long)num2, 0x8eacL);
            int num4 = this.mod((long)num3, 0x5b5L);
            int num5 = this.mod((long)num4, 0x16dL) + (0x16d * this.quotient((long)num4, 0x5b4L));
            int num6 = this.quotient((long)num4, 0x447L);
            int year = ((((((400 * this.quotient((long)(j - 0x1a4452), 0x23ab1L)) + (100 * this.quotient((long)num2, 0x8eacL))) + (4 * this.quotient((long)num3, 0x5b5L))) + this.quotient((long)num4, 0x16dL)) - this.quotient((long)num4, 0x5b4L)) - this.quotient((long)num, 0xb2574L)) + 1;
            int num9 = this.quotient((long)((0x16c + num6) - num5), 0x132L);
            int num10 = (num9 * (this.quotient((long)num5, (long)0x1f) + 1)) + ((1 - num9) * (this.quotient((long)((5 * (num5 - num6)) + 13), 0x99L) + 1));
            num5 += 1 - this.quotient((long)num, 0xb2574L);
            int num11 = num5;
            if ((num3 != 0) || (num5 != 0))
            {
                num1 = 1;
            }
            else
            {
                num1 = Convert.ToInt32(num2 == 0);
            }
            if (num1 == 0)
            {
                num10 = 12;
                num11 = 0x1f;
            }
            else
            {
                monthDays[2] = this.isGregorianLeap(year) ? 0x1d : 0x1c;
                int index = 1;
                while (true)
                {
                    if (index <= nMonths)
                    {
                        if (num5 > monthDays[index])
                        {
                            num5 -= monthDays[index];
                            index++;
                            continue;
                        }
                        num11 = num5;
                    }
                    break;
                }
            }
            return new int[] { year, num10, num11 };
        }

        private int mod(long i, long j)
        {
            return (int)(i - (j * this.quotient(i, j)));
        }

        private int quotient(long i, long j)
        {
            return (int)Math.Floor((double)(((double)i) / ((double)j)));
        }

        public void set(int year, int month, int day)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.dateIsUnset = false;
        }

        public void set(int year, int month, int day, int era)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.setEra(era);
            this.dateIsUnset = false;
        }

        public void setEra(int era)
        {
            if (-285019 == era)
            {
                this.jdoffset = era;
            }
            this.jdoffset = 0x1a4dd0;
        }

        public void unset()
        {
            this.unsetEra();
            this.year = -1;
            this.month = -1;
            this.day = -1;
            this.dateIsUnset = true;
        }

        public void unsetEra()
        {
            this.jdoffset = -1;
        }


    }
}