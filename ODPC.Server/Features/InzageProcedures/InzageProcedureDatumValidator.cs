using Microsoft.AspNetCore.Mvc.ModelBinding;
using ODPC.Features.Publicaties;

namespace ODPC.Features.InzageProcedures
{
    public static class InzageProcedureDatumValidator
    {
        public static bool IsValid(InzageProcedure inzageProcedure, Publicatie? publicatie, ModelStateDictionary modelState)
        {
            if (publicatie?.GepubliceerdOp is DateTimeOffset gepubliceerdOp)
            {
                var minDatumBeginInzagetermijn = DateOnly.FromDateTime(gepubliceerdOp.Date);

                if (inzageProcedure.DatumBeginInzagetermijn < minDatumBeginInzagetermijn)
                {
                    modelState.AddModelError(
                        nameof(InzageProcedure.DatumBeginInzagetermijn),
                        "Begindatum inzagetermijn moet op of na de publicatiedatum liggen");

                    return false;
                }
            }

            if (inzageProcedure.DatumEindeInzagetermijn <= inzageProcedure.DatumBeginInzagetermijn)
            {
                modelState.AddModelError(
                    nameof(InzageProcedure.DatumEindeInzagetermijn),
                    "Einddatum inzagetermijn moet na de begindatum liggen");

                return false;
            }

            return true;
        }
    }
}
