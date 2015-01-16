using System;

using Java.Util;
using Java.Lang;

using Com.Google.Android.Gms.Plus.Model.Moments;

namespace GooglePlusClient
{
   class MomentUtil
   {
      // A mapping of moment type to target URL.
      public static HashMap<String, String> MOMENT_TYPES;

      // A list of moment target types.
      public static ArrayList<String> MOMENT_LIST;
      public static String[] VISIBLE_ACTIVITIES;

      static MomentUtil()
      {
         MOMENT_TYPES = new HashMap<String, String>(9);
         MOMENT_TYPES.Put("AddActivity",
                 "https://developers.google.com/+/plugins/snippet/examples/thing");
         MOMENT_TYPES.Put("BuyActivity",
                 "https://developers.google.com/+/plugins/snippet/examples/a-book");
         MOMENT_TYPES.Put("CheckInActivity",
                 "https://developers.google.com/+/plugins/snippet/examples/place");
         MOMENT_TYPES.Put("CommentActivity",
                 "https://developers.google.com/+/plugins/snippet/examples/blog-entry");
         MOMENT_TYPES.Put("CreateActivity",
                 "https://developers.google.com/+/plugins/snippet/examples/photo");
         MOMENT_TYPES.Put("ListenActivity",
                 "https://developers.google.com/+/plugins/snippet/examples/song");
         MOMENT_TYPES.Put("ReserveActivity",
                 "https://developers.google.com/+/plugins/snippet/examples/restaurant");
         MOMENT_TYPES.Put("ReviewActivity",
                 "https://developers.google.com/+/plugins/snippet/examples/widget");

         MOMENT_LIST = new ArrayList<String>(MomentUtil.MOMENT_TYPES.KeySet());
         Collections.Sort(MOMENT_LIST);

         VISIBLE_ACTIVITIES = MOMENT_TYPES.KeySet().ToArray(new String[0]);
         int count = VISIBLE_ACTIVITIES.Length;
         for (int i = 0; i < count; i++)
         {
            VISIBLE_ACTIVITIES[i] = "http://schemas.google.com/" + VISIBLE_ACTIVITIES[i];
         }
      }

      // Generates the "result" JSON object for select moments.
      public static IItemScope getResultFor(String momentType)
      {
         if (momentType.Equals("CommentActivity"))
         {
            return getCommentActivityResult();
         }
         if (momentType.Equals("ReserveActivity"))
         {
            return getReserveActivityResult();
         }
         if (momentType.Equals("ReviewActivity"))
         {
            return getReviewActivityResult();
         }
         return null;
      }

      // Generates the "result" JSON object for CommentActivity moment.
      private static IItemScope getCommentActivityResult()
      {
         return new IItemScope_Builder()
             .SetType("http://schema.org/Comment")
             .SetUrl("https://developers.google.com/+/plugins/snippet/examples/blog-entry#comment-1")
             .SetName("This is amazing!")
             .SetText("I can't wait to use it on my site!")
             .Build();
      }

      // Generates the "result" JSON object for ReserveActivity moment.
      private static IItemScope getReserveActivityResult()
      {
         return new IItemScope_Builder()
             .SetType("http://schemas.google.com/Reservation")
             .SetStartDate("2012-06-28T19:00:00-08:00")
             .SetAttendeeCount(3)
             .Build();
      }

      // Generates the "result" JSON object for ReviewActivity moment.
      private static IItemScope getReviewActivityResult()
      {
         IItemScope rating = new IItemScope_Builder()
             .SetType("http://schema.org/Rating")
             .SetRatingValue("100")
             .SetBestRating("100")
             .SetWorstRating("0")
             .Build();

         return new IItemScope_Builder()
             .SetType("http://schema.org/Review")
             .SetName("A Humble Review of Widget")
             .SetUrl("https://developers.google.com/+/plugins/snippet/examples/review")
             .SetText("It is amazingly effective")
             .SetReviewRating(rating)
             .Build();
      }
   }
}
