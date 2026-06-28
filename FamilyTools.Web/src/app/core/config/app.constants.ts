export class AppSettings {
    private static API_URL="api/"

    private static EASYCOMPTA_URL=`${this.API_URL}easycompta/`
    public static USER_URL=`${this.API_URL}User/`

    public static TAG_URL=`${this.EASYCOMPTA_URL}AccountTag/`

    public static ENTER_URL=`${this.EASYCOMPTA_URL}AccountEnter/`

    public static PAGE_URL=`${this.EASYCOMPTA_URL}AccountPage/`

    public static IMPORT_CSV_URL=`${this.EASYCOMPTA_URL}ImportCSV`

    public static LINE_URL=`${this.EASYCOMPTA_URL}AccountLine/`

    public static LINES_URL=`${this.EASYCOMPTA_URL}AccountLines/`

    public static PAYMENTDONE_URL=`${this.EASYCOMPTA_URL}PaymentDone/`

  public static OPERATIONSTYPES_URL=`${this.EASYCOMPTA_URL}OperationType/`

}
