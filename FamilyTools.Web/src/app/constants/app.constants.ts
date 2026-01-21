export const OPERATIONTYPESTRING: string[] = [
    "Inconnu",
    "Prélèvement",
    "Paiement Carte",
    "Virement Recu",
    "Virement Emis",
    "Remboursement Prêt",
    "Réglement",
    "Avoir",
    "Cotisation",
    "Chèque emis",
]

export class AppSetings {
    private static API_URL="api/"
    
    private static EASYCOMPTA_URL=`${this.API_URL}easycompta/`
    public static USER_URL=`${this.API_URL}User/`
    
    public static TAG_URL=`${this.EASYCOMPTA_URL}AccountTag/`
    
    public static ENTER_URL=`${this.EASYCOMPTA_URL}AccountEnter/`
    
    public static PAGE_URL=`${this.EASYCOMPTA_URL}AccountPage/`
    
    public static LINE_URL=`${this.EASYCOMPTA_URL}AccountLine/`
    
    public static PAYMENTDONE_URL=`${this.EASYCOMPTA_URL}PaymentDone/`

}