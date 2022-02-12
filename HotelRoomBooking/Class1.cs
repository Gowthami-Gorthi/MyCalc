
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelRoomBooking
{
    public class Class1
    {
        Hotel_BookingEntities h=new Hotel_BookingEntities();
        //Login for the Hotel Room Booking
        public bool Login(string emailId, string cpassword)
        {
            string res = "";
            if (cpassword != null)
            {
                 byte[] encode = Encoding.ASCII.GetBytes(cpassword);
                for (int i = 0; i < encode.Length; i++)
                {
                    res += encode[i].ToString();
                }                
                return (from t in h.customers
                        where t.emailId == emailId && t.cpassword == res
                        select t).Count() > 0;
            }
            return false;           
        } 
        //Registration for the Hotel Room Booking
        public string Registration(customer c)
        {
            string res = "";
            byte[] encode = Encoding.ASCII.GetBytes(c.cpassword);
            for(int i = 0; i < encode.Length; i++)
            {
                res+=encode[i].ToString();
            }
            c.cpassword = res;
            h.customers.Add(c);
            int cou = h.SaveChanges();
            if(cou > 0)
            {
                return cou.ToString();
            }
            else
            {
                return "Registration failed";
            }                          
        }
        //Forgot password
        public bool Forgot(string emailId,string cpassword,string cpwd)
        {
            int c = 0;
            var res = (from t in h.customers where t.emailId == emailId select t).FirstOrDefault();
            if (cpassword == cpwd)
            {
                string result = "";
                byte[] encode = Encoding.ASCII.GetBytes(cpassword);
                for (int i = 0; i < encode.Length; i++)
                {
                    result += encode[i].ToString();
                }
                if (res != null)
                {
                    res.cpassword = result;
                   c= h.SaveChanges();
                }
            }
            if (c > 0)
            {
                return true;
            }
            return false;
        }
        //Search Box
        public List<Hotel> Search(string text)
        {
            return (from t in h.Hotels where t.place==text select t).ToList();
        }
        //What types of Rooms present in that particular Hotel
        public List<HMap> SelectHotel(Hotel hotel)
        {
            var res = from t in h.HMaps where t.HotelName == hotel.HotelName select t;
            return res.ToList();
        }
        public bool CheckAvailabaility(int rid)
        {
            var res = from t in h.BookingHistories where t.Rid==rid select t;
            if(res.Count() <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string Addmember(AddMember s1)
        {
            if (s1 != null)
            {
                h.AddMembers.Add(s1);
                int i = h.SaveChanges();
                if (i > 0)
                {
                    return "Member Inserted successfully!!";
                }
                else
                {
                    return "Error Occured!";
                }
            }
            return "please enter values";
        }
        //Book a room by filling this form
        public int Booking(Booking b)
        {
            h.Bookings.Add(b);
            return h.SaveChanges();
        }
        public int History(BookingHistory bH)
        {
             var res = (from t in h.HMaps where t.Rid == bH.Rid select new { t.HotelName, t.RoomType }).FirstOrDefault();
            bH.HotelName= res.HotelName;
            bH.RoomType= res.RoomType;
            h.BookingHistories.Add(bH);
            return h.SaveChanges();
        }
        Stack<BookingHistory> BookHist=new Stack<BookingHistory>();
        public List<BookingHistory> Display(string emailId)
        {
            var res = from t in h.BookingHistories where t.emailId == emailId select t;
            return res.ToList();
        }
        /* public int BookingRegisteration(Booking b)
         {

             h.Bookings.Add(b);

             return h.SaveChanges();

         }

 */
        /*  public string BookingRegistration(HMap hmap)
 {
    // var res = from t in h.Bookings join r in hmap on t.Rid equals hmap.Rid select new {t.Bid ,t.Bname,t.emailId};
     return res.ToString();
 }*/
        //If the user done payment by using cards
        public string Paymentpage(Payment card, int price)
        {
            var result = (from t in h.Banks
                          where t.card_Num == card.Card_Number && t.Account_HName == card.Account_HName && t.Expiry_Date == card.Expiry_Date
                          && t.cvv == card.cvv
                          select t).FirstOrDefault();
            if (result != null)
            {
                if (result.Balance > price)
                {
                    result.Balance = result.Balance - price;
                    var myAct = h.Banks.Find(457896240584);
                    myAct.Balance = myAct.Balance + price;
                    h.SaveChanges();
                    h.Payments.Add(card);
                    try
                    {
                        return h.SaveChanges().ToString();
                }catch (DbUnexpectedValidationException e)
                {
                    return "Something went wrong";
                }
            }
            else
                {
                    return "You don't have sufficient balance";
                }
            }
            return "payment failed";
        }
        //if the user done payment by using UPI
        public string Upi(UPI upi,int price)
        {
            var result = (from t in h.Banks
                          where t.UPI_ID == upi.UPI_ID
                          select t).FirstOrDefault();
            if (result != null)
            {
                if (result.Balance > price)
                {
                   result.Balance = result.Balance - price;
                    var myAct = h.Banks.Find(457896240584);
                    myAct.Balance = myAct.Balance + price;
                    h.SaveChanges();
                    h.UPIs.Add(upi);
                    return h.SaveChanges().ToString();
                }
                else
                {
                    return "You don't have sufficient balance";
                }               
            }
            return "payment failed";
        }
        public string AddRoom()
        {
            var res=(from t in h.HMaps select t).Count();
            if (res >0)
            {
                return res.ToString();
            }
            else
            {
                return "No room booked";
            }
        }
        public HashSet<price> Facility()
        {
            return h.prices.ToHashSet();
        }
        //If all room Booking and payment is successfully ,generate a Receipt by using PaymentBill
        public List<ReceiptFor_Result> Receipt(int rid)
        {
            var res = h.ReceiptFor(rid);
            return res.ToList<ReceiptFor_Result>();   
        }
        //User Reviews
        Stack<FeedBack> feedBacks = new Stack<FeedBack>();
        public Stack<FeedBack> Reviews(FeedBack f)
        {
            var search = from t in h.FeedBacks where t.emailId== f.emailId select t;
            if(search.Count() ==0)
            {
                h.FeedBacks.Add(f);
                h.SaveChanges();
                foreach (var item in h.FeedBacks)
                {
                    feedBacks.Push(item);
                }                
            }
            return feedBacks; 
        }
        //User profile
        public List<customer> Profile(string emailId)
        {
            var res=from t in h.customers where t.emailId==emailId select t;
          //  if (res.Count() > 0)
            {
                return res.ToList();
            }
        }
        public int EditProfile(customer c)
        {
            h.Entry(c).State = EntityState.Modified;                            
            return h.SaveChanges();
        }
        public int CancelBooking(int BId)
        {
            var res = (from t in h.BookingHistories where t.BhId == BId select t).FirstOrDefault();
            PaymentAdmin(res);
            h.BookingHistories.Remove(res);
            return h.SaveChanges();
        }
        //If the user done payment by using cards
        public int PaymentAdmin(BookingHistory res)
        {
            var myAct = h.Banks.Find(457896240584);
            var result = h.Banks.Find(245148756952);
            if (myAct != null)
            {
                if (myAct.Balance > res.Amount)
                {
                    myAct.Balance = myAct.Balance - res.Amount;
                    result.Balance = result.Balance + res.Amount;
                }               
            }
            return h.SaveChanges();
        }
        public bool AdminLogin(string admin, string cpassword)
        {
            if (admin=="admin" && cpassword=="india123")
            {
                return true;
            }
            return false;
        }

        public bool ForgotAdmin(string admin, string cpassword, string cpwd)
        {
            if (cpassword == cpwd)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public List<customer> UserDetails()
        {
            var res =h.customers;
            return res.ToList();
        }

        Stack<BookingHistory> bookingHistories = new Stack<BookingHistory>();
        public Stack<BookingHistory> BookingHistory()
        {
            foreach (var i in h.BookingHistories)
            {
                bookingHistories.Push(i);
            }
            return bookingHistories;
        }
        public List<Hotel> Hotels()
        {
            return h.Hotels.ToList();
        }
        public int Hcreate(Hotel hotel)
        {
            h.Hotels.Add(hotel);
            return h.SaveChanges();
        }

        public Hotel HEdit(string hotelName)
        {
            var res=(from t in h.Hotels where t.HotelName == hotelName select t).FirstOrDefault();
            return res;
        }

        public int HEdited(Hotel hotel)
        {
            h.Entry(hotel).State = EntityState.Modified;
            return h.SaveChanges();
        }
        public int HDeleted(string HotelName)
        {
            var res = from t in h.Hotels where t.HotelName ==HotelName select t;
            foreach(var item in res)
                {
                    h.Hotels.Remove(item);
                }               
           return h.SaveChanges();
        }
        public List<HMap> HDetails(string hotelName)
        {
            var res=from t in h.HMaps where t.HotelName==hotelName select t;
            return res.ToList();
        }

        public int Rcreate(HMap hMap)
        {
            h.HMaps.Add(hMap);
            return h.SaveChanges();
        }

        public HMap REdit(int Rid)
        {
            var res = (from t in h.HMaps where t.Rid == Rid select t).FirstOrDefault();
            return res;
        }

        public int REdited(HMap hMap)
        {
            h.Entry(hMap).State = EntityState.Modified;
            return h.SaveChanges();
        }

        public int RDeleted(int Rid)
        {
            var res = (from t in h.HMaps where t.Rid == Rid select t).FirstOrDefault();
               h.HMaps.Remove(res);
            return h.SaveChanges();
        }

        public List<HMap> RDetails(int rid)
        {
                var res = from t in h.HMaps where t.Rid == rid select t;
                return res.ToList();           
        }

        public int Contact(string uname, string text)
        {
            Contact c = new Contact();
            c.emailId = uname;
            c.Query = text;
            c.Cdate = DateTime.Now;
            h.Contacts.Add(c);            
            return h.SaveChanges();
        }
        public List<Contact> UserQueries()
        {
            return h.Contacts.ToList();
        }
    }
}
