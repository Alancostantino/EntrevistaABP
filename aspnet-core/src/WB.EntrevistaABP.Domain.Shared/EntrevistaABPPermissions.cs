namespace WB.EntrevistaABP.Permissions;

public static class EntrevistaABPPermissions
{
    public const string GroupName = "EntrevistaABP";
    public static class Viajes
    {
        //DEFINIMOS CONSTANTES EN CLASE STATIC ( NO PUEDE SER INSTANCIADA, SIRVE COMO MODULO)
        // Permiso base (acceso/listado/ver menú)
        public const string Default = GroupName + ".Viajes";

        // Permisos específicos (acciones)
        public const string Create = GroupName + ".Viajes.Create";
        public const string Update = GroupName + ".Viajes.Update";
        public const string Delete = GroupName + ".Viajes.Delete";
        public const string ManagePassengers = GroupName + ".Viajes.ManagePassengers";
    }

}
