//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CautaRetete.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class RecipeSpices
    {
        public int RecipeId { get; set; }
        public int SpiceId { get; set; }

        public Tuple<int, int> GetRecipeSpices()
        {
            return new Tuple<int, int>(RecipeId, SpiceId);
        }
    }
}
