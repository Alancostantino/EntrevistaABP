namespace WB.EntrevistaABP.Permissions;

public static class EntrevistaABPPermissions
{
    public const string GroupName = "Viajes";
    public static class Viajes
    {
        //DEFINIMOS CONSTANTES EN CLASE STATIC ( NO PUEDE SER INSTANCIADA, SIRVE COMO MODULO)
        public const string Default = GroupName + ".Default"; 
        public const string Create  = GroupName + ".Create";
        public const string Update  = GroupName + ".Update";
        public const string Delete  = GroupName + ".Delete";
        public const string ManagePassengers = GroupName + ".ManagePassengers";
    }
    
}
