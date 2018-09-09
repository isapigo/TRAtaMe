' ------------------------------------------------------------------------------
' Autora: Isabel Piñuel González 
' Este proyecto es parte del TFG del Curso de Adaptación al Grado de Informática
' Universidad Internacional de la Rioja (UNIR)
' Este código se ofrece bajo licencia MIT
' ------------------------------------------------------------------------------
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.ComponentModel
Imports System.Data
Imports System.IO
Imports System.Net
Imports TRAtaMeWS.org.madrid.salud.gestionai

' Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la siguiente línea.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="http://hcrTRAtaMeWS")> _
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<ToolboxItem(False)> _
Public Class TRAtaMeWS
    Inherits System.Web.Services.WebService

    <WebMethod()> _
    Public Function valida_entrada(ByVal UserName As String, ByVal Password As String) As cls_usuario

        Dim usuario As New cls_usuario

        usuario = Funciones.valida_usuario(UserName, Password)

        Return usuario

    End Function

    <WebMethod()> _
    Public Function dame_img_aud(ByVal tipo As String, ByVal el_Where_Order As String, ByVal datos_informacion As cls_informacion) As cls_informacion

        dame_img_aud = datos_informacion

        If tipo = "A" Then
            dame_img_aud.oigo = 0
            dame_img_aud.oigo_enlace = ""
        Else
            dame_img_aud.veo = 0
            dame_img_aud.veo_enlace = ""
        End If

        Dim str_SQL As String = "SELECT id, enlace" & _
                    " FROM fichero" & _
                    " WHERE idepisodio = " & datos_informacion.id_episodio & _
                    " AND tipofic = '" & tipo & "'" & _
                    " AND " & el_Where_Order
        Try
            Using Conn As New SqlClient.SqlConnection

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe

                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader

                If Devuelve.HasRows Then
                    Do While Devuelve.Read
                        If tipo = "A" Then
                            dame_img_aud.oigo = Devuelve.GetValue(0)
                            dame_img_aud.oigo_enlace = Devuelve.GetValue(1)
                        Else
                            dame_img_aud.veo = Devuelve.GetValue(0)
                            dame_img_aud.veo_enlace = Devuelve.GetValue(1)
                        End If
                        Exit Do
                    Loop
                End If

                Devuelve.Close()

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            inserta_log("WebService dame_img_aud: ", datos_informacion.id_episodio, ex.Message)
        End Try

        Return dame_img_aud

    End Function

    <WebMethod()> _
    Public Function toda_informacion(ByVal episodio As String, _
                                           ByRef paciente As cls_paciente, _
                                           ByRef informacion As cls_informacion, _
                                           ByRef mensaje As String) As Boolean

        toda_informacion = False

        Try
            'Valida que el codigo corresponda a uno válido
            Using Conn As New SqlClient.SqlConnection
                Dim where_codser As String = "AND 1=1 "

                If Len(informacion.codser) > 0 Then
                    where_codser = "AND c.codser = '" & informacion.codser & "'"
                End If

                Dim str_SQL As String
                str_SQL = "SELECT b.id, b.idpaciente, a.nhc, RTRIM(a.nombre) + ' '+ RTRIM(a.apellido1) + ' '+ RTRIM(a.apellido2) As nombre, " &
                          "a.dni, a.cipa, a.numtel1, b.fecha, c.codser, c.doctor, b.pregunta, c.respuesta, a.fecnac " &
                          "FROM paciente a, episodio b LEFT JOIN respuesta c ON c.idepisodio = b.id " &
                          where_codser &
                          "WHERE(b.idpaciente = a.id) " &
                          "AND b.id = " & episodio & ";"

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader

                If Devuelve.HasRows Then
                    Do While Devuelve.Read

                        paciente.id_paciente = Trim(mira_nulo(Devuelve.GetValue(1)))
                        paciente.nhc = Trim(mira_nulo(Devuelve.GetValue(2)))
                        paciente.nombre_com = Trim(mira_nulo(Devuelve.GetValue(3)))
                        paciente.dni = Trim(mira_nulo(Devuelve.GetValue(4)))
                        paciente.cipa = Trim(mira_nulo(Devuelve.GetValue(5)))
                        paciente.telefono = Trim(mira_nulo(Devuelve.GetValue(6)))
                        paciente.fecnac = Trim(mira_nulo(Devuelve.GetValue(12)))

                        informacion.fecha = Trim(mira_nulo(Devuelve.GetValue(7)))
                        informacion.id_episodio = Trim(mira_nulo(Devuelve.GetValue(0)))
                        informacion.id_paciente = paciente.id_paciente
                        informacion.nhc = paciente.nhc
                        informacion.codser = Trim(mira_nulo(Devuelve.GetValue(8)))
                        informacion.fecha = informacion.fecha
                        informacion.doctor = Trim(mira_nulo(Devuelve.GetValue(9)))
                        informacion.pregunta = Trim(mira_nulo(Devuelve.GetValue(10)))
                        informacion.respuesta = Trim(mira_nulo(Devuelve.GetValue(11)))
                        informacion.veo = informacion.veo
                        informacion.oigo = informacion.oigo

                        toda_informacion = True
                        Exit Do
                    Loop
                End If
                Devuelve.Close()

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensaje = ex.Message
            inserta_log("WebService", informacion.id_episodio, ex.Message)
        End Try

        Return toda_informacion

    End Function

    <WebMethod()> _
    Public Function datos_paciente_HCIS(ByVal nhc As Integer, ByVal dni As String, ByRef mensaje As String) As cls_paciente
        mensaje = ""

        Dim dat_paciente As New cls_paciente
        dat_paciente.id_paciente = 0
        dat_paciente.nhc = 0
        dat_paciente.nombre_com = ""
        dat_paciente.dni = ""
        dat_paciente.cipa = ""
        dat_paciente.telefono = ""
        dat_paciente.domicilio = ""

        datos_paciente_HCIS = dat_paciente

        Try
            Using Conn As New System.Data.Odbc.OdbcConnection

                Dim or_dni As String = ")"

                If dni <> "SIN_DNI" And IsNumeric(dni) Then
                    or_dni = " OR a.dni = '" & dni & "')"
                End If

                Dim str_SQL As String
                str_SQL = "SELECT a.numerohc, a.nombre, a.apellid1, a.apellid2, a.dni, a.letranif, a.telefono,  a.domiresi, a.codipost, a.cip_auto, a.num, a.provresi, a.poblares, b.nompobla, c.descprov, a.fechanac, a.telefono2 " & _
                            "FROM informix.pacientes a, outer poblacion b, outer provincias c " & _
                           "WHERE (a.numerohc = " & nhc & _
                               or_dni & _
                           "AND b.codiprov = a.provresi " & _
                           "AND b.codpobla = a.poblares " & _
                           "AND c.codprov = a.provresi;"

                Conn.ConnectionString = My.Settings.cadena_conexion_clihis
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New Odbc.OdbcCommand(str_SQL, Conn)

                Using Devuelve As Odbc.OdbcDataReader = Sentencia.ExecuteReader
                    If Devuelve.HasRows Then
                        Do While Devuelve.Read
                            datos_paciente_HCIS.nhc = Trim(mira_nulo(Devuelve.GetValue(0)))
                            datos_paciente_HCIS.nombre_com = Trim(mira_nulo(Devuelve.GetValue(1))) & " " & Trim(mira_nulo(Devuelve.GetValue(2))) & " " & Trim(mira_nulo(Devuelve.GetValue(3)))
                            datos_paciente_HCIS.dni = mira_nulo(Devuelve.GetValue(4)) & Trim(mira_nulo(Devuelve.GetValue(5)))
                            datos_paciente_HCIS.telefono = mira_nulo(Devuelve.GetValue(6))
                            datos_paciente_HCIS.domicilio = mira_nulo(Devuelve.GetValue(7))
                            datos_paciente_HCIS.codpost = mira_nulo(Devuelve.GetValue(8))
                            datos_paciente_HCIS.cipa = mira_nulo(Devuelve.GetValue(9))
                            datos_paciente_HCIS.numero = mira_nulo(Devuelve.GetValue(10))
                            datos_paciente_HCIS.codpro = mira_nulo(Devuelve.GetValue(11))
                            datos_paciente_HCIS.codmun = mira_nulo(Devuelve.GetValue(12))
                            datos_paciente_HCIS.municipio = mira_nulo(Devuelve.GetValue(13))
                            datos_paciente_HCIS.provincia = mira_nulo(Devuelve.GetValue(14))
                            datos_paciente_HCIS.fecnac = mira_nulo(Devuelve.GetValue(15))
                            datos_paciente_HCIS.nombre = Trim(mira_nulo(Devuelve.GetValue(1)))
                            datos_paciente_HCIS.apellido1 = Trim(mira_nulo(Devuelve.GetValue(2)))
                            datos_paciente_HCIS.apellido2 = Trim(mira_nulo(Devuelve.GetValue(3)))
                            datos_paciente_HCIS.numtel2 = Trim(mira_nulo(Devuelve.GetValue(16)))
                            datos_paciente_HCIS.tipodom = "V"
                            datos_paciente_HCIS.calle = datos_paciente_HCIS.domicilio
                        Loop
                    End If
                End Using
                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using


        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensaje = Trim(ex.Message)
            inserta_log("WebService", 0, mensaje)
        End Try

        Return datos_paciente_HCIS

    End Function

    <WebMethod()> _
    Public Function actualiza_paciente(ByVal datos_paciente As cls_paciente, ByRef mensaje As String) As Boolean

        mensaje = ""

        Try
            Using Conn As New SqlClient.SqlConnection
                While Len(datos_paciente.dni) < 9
                    datos_paciente.dni = "0" & Trim(datos_paciente.dni)
                End While

                Dim str_SQL As String = "UPDATE paciente " & _
                                                " SET" & _
                                                " nhc = '" & datos_paciente.nhc & "'," & _
                                                " dni = '" & datos_paciente.dni & "'," & _
                                                " cipa = '" & datos_paciente.cipa & "'," & _
                                                " nombre = '" & datos_paciente.nombre & "'," & _
                                                " apellido1 = '" & datos_paciente.apellido1 & "'," & _
                                                " apellido2 = '" & datos_paciente.apellido2 & "'," & _
                                                " fecnac = '" & datos_paciente.fecnac & "'," & _
                                                " calle = '" & datos_paciente.calle & "'," & _
                                                " numero = '" & datos_paciente.numero & "'," & _
                                                " codpos = '" & datos_paciente.codpost & "'," & _
                                                " codpro = " & datos_paciente.codpro & "," & _
                                                " provincia = '" & datos_paciente.provincia & "'," & _
                                                " codmun = " & datos_paciente.codmun & "," & _
                                                " municipio = '" & datos_paciente.municipio & "'," & _
                                                " numtel1 = '" & datos_paciente.telefono & "'," & _
                                                " numtel2 = '" & datos_paciente.numtel2 & "'," & _
                                                " tipodom = '" & datos_paciente.tipodom & "'" & _
                                                " WHERE id = " & datos_paciente.id_paciente

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim filas As Integer = Sentencia.ExecuteNonQuery

                If filas = 0 Then
                    actualiza_paciente = False
                Else
                    actualiza_paciente = True

                    mensaje = "Actualiza paciente (Id:" & datos_paciente.id_paciente & ") en el sistema."
                    inserta_log("WebService", datos_paciente.id_paciente, mensaje)

                End If

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensaje = Trim(ex.Message)
            inserta_log("WebService", 0, mensaje)
        End Try

        Return actualiza_paciente

    End Function

    <WebMethod()> _
    Public Function datos_paciente(ByVal episodio As Integer, ByVal id_paciente As Integer, ByRef mensaje As String) As cls_paciente

        mensaje = ""

        Dim dat_paciente As New cls_paciente
        dat_paciente.id_paciente = 0
        dat_paciente.nhc = 0
        dat_paciente.nombre_com = ""
        dat_paciente.dni = ""
        dat_paciente.cipa = ""
        dat_paciente.telefono = ""

        datos_paciente = dat_paciente
        Try
            'Valida que el codigo corresponda a uno válido
            Using Conn As New SqlClient.SqlConnection

                Dim str_SQL As String = ""

                Select Case True
                    Case id_paciente = 0
                        str_SQL = "SELECT DISTINCT a.id as id_paciente, RTRIM(a.nombre) + ' '+ RTRIM(a.apellido1) + ' '+ RTRIM(a.apellido2) As nombre_com, a.cipa, a.numtel1, a.nhc, a.dni, a.fecnac, " & _
                                                  "a.calle, a.codpos, a.numero, a.codpro, a.codmun, a.municipio, a.provincia, " & _
                                                  "a.nombre, a.apellido1, a.apellido2, a.numtel2, a.tipodom, a.calle " & _
                                                  "FROM paciente a, episodio b " & _
                                                 "WHERE b.id = " & episodio & " " & _
                                                   "AND b.idpaciente = a.id;"
                    Case episodio = 0
                        str_SQL = "SELECT a.id as id_paciente, RTRIM(a.nombre) + ' '+ RTRIM(a.apellido1) + ' '+ RTRIM(a.apellido2) As nombre_com, a.cipa, a.numtel1, a.nhc, a.dni, a.fecnac, " & _
                                                  "a.calle, a.codpos, a.numero, a.codpro, a.codmun, a.municipio, a.provincia, " & _
                                                  "a.nombre, a.apellido1, a.apellido2, a.numtel2, a.tipodom, a.calle " & _
                                                  "FROM paciente a " & _
                                                 "WHERE a.id = " & id_paciente & ";"
                End Select

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader

                If Devuelve.HasRows Then
                    Do While Devuelve.Read
                        datos_paciente.id_paciente = Trim(mira_nulo(Devuelve.GetValue(0)))
                        datos_paciente.nhc = Trim(mira_nulo(Devuelve.GetValue(4)))
                        datos_paciente.nombre_com = Trim(mira_nulo(Devuelve.GetValue(1)))
                        datos_paciente.dni = Trim(mira_nulo(Devuelve.GetValue(5)))
                        datos_paciente.cipa = Trim(mira_nulo(Devuelve.GetValue(2)))
                        datos_paciente.telefono = Trim(mira_nulo(Devuelve.GetValue(3)))
                        datos_paciente.fecnac = Trim(mira_nulo(Devuelve.GetValue(6)))
                        datos_paciente.domicilio = Trim(mira_nulo(Devuelve.GetValue(7)))
                        datos_paciente.codpost = mira_nulo(Devuelve.GetValue(8))
                        datos_paciente.numero = mira_nulo(Devuelve.GetValue(9))
                        datos_paciente.codpro = mira_nulo(Devuelve.GetValue(10))
                        datos_paciente.codmun = mira_nulo(Devuelve.GetValue(11))
                        datos_paciente.municipio = Trim(mira_nulo(Devuelve.GetValue(12)))
                        datos_paciente.provincia = Trim(mira_nulo(Devuelve.GetValue(13)))
                        datos_paciente.nombre = Trim(mira_nulo(Devuelve.GetValue(14)))
                        datos_paciente.apellido1 = Trim(mira_nulo(Devuelve.GetValue(15)))
                        datos_paciente.apellido2 = Trim(mira_nulo(Devuelve.GetValue(16)))
                        datos_paciente.numtel2 = Trim(mira_nulo(Devuelve.GetValue(17)))
                        datos_paciente.tipodom = Trim(mira_nulo(Devuelve.GetValue(18)))
                        datos_paciente.calle = Trim(mira_nulo(Devuelve.GetValue(19)))
                        Exit Do
                    Loop
                Else
                    mensaje = "Paciente no encontrado."
                End If
                Devuelve.Close()

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            inserta_log("WebService", 0, ex.Message)

        End Try

        Return datos_paciente

    End Function

    <WebMethod()> _
    Public Function nuevo_paciente(ByRef datos_paciente As cls_paciente, ByRef mensaje As String) As Boolean

        mensaje = ""

        Try
            Using Conn As New SqlClient.SqlConnection

                Dim str_SQL As String = "INSERT INTO paciente " & _
                                                "(nhc,dni,cipa,nombre,apellido1,apellido2,fecnac,calle,numero,codpos,codpro,provincia," & _
                                                "codmun,municipio,numtel1,numtel2,tipodom)" & _
                                                " VALUES " & _
                                                "('" & datos_paciente.nhc & "'," & _
                                                " '" & datos_paciente.dni & "'," & _
                                                " '" & datos_paciente.cipa & "'," & _
                                                " '" & datos_paciente.nombre & "'," & _
                                                " '" & datos_paciente.apellido1 & "'," & _
                                                " '" & datos_paciente.apellido2 & "'," & _
                                                " '" & datos_paciente.fecnac & "'," & _
                                                " '" & datos_paciente.calle & "'," & _
                                                " '" & datos_paciente.numero & "'," & _
                                                " '" & datos_paciente.codpost & "'," & _
                                                " " & datos_paciente.codpro & "," & _
                                                " '" & datos_paciente.provincia & "'," & _
                                                " " & datos_paciente.codmun & "," & _
                                                " '" & datos_paciente.municipio & "'," & _
                                                " '" & datos_paciente.telefono & "'," & _
                                                " '" & datos_paciente.numtel2 & "'," & _
                                                " '" & datos_paciente.tipodom & "')"

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim filas As Integer = Sentencia.ExecuteNonQuery

                If filas = 0 Then
                    nuevo_paciente = False
                Else
                    'Recupera el autonumerico
                    Sentencia.CommandText = "SELECT @@IDENTITY FROM paciente;"
                    datos_paciente.id_paciente = Sentencia.ExecuteScalar()
                    nuevo_paciente = True

                    mensaje = "Nuevo paciente (Id:" & datos_paciente.id_paciente & ") incluido en el sistema."
                    inserta_log("WebService", datos_paciente.id_paciente, mensaje)

                End If

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensaje = ex.Message
            inserta_log("WebService", 0, mensaje)
        End Try

        Return nuevo_paciente

    End Function

    <WebMethod()> _
    Public Function paciente_en_TRA(ByVal nhc As String, ByRef mensaje As String) As Boolean

        mensaje = ""

        Try

            Dim fecha_desde As Date = Now()

            Dim str_SQL As String = "SELECT nhc, nombre, fdia, codser " & _
                                      "FROM gq_preprog " & _
                                     "WHERE fdia <= '" & Format(fecha_desde, "dd/MM/yyyy") & "' " & _
                                       "AND fdia + 30 >= '" & Format(fecha_desde, "dd/MM/yyyy") & "' " & _
                                       "AND nhc = '" & nhc & "' " & _
                                       "AND codser = 'TRA';"
            Using Conn As New System.Data.Odbc.OdbcConnection

                Conn.ConnectionString = My.Settings.cadena_conexion_clihis
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New Odbc.OdbcCommand(str_SQL, Conn)

                Using Devuelve As Odbc.OdbcDataReader = Sentencia.ExecuteReader

                    If Devuelve.HasRows Then
                        Do While Devuelve.Read
                            paciente_en_TRA = True
                            Exit Do
                        Loop
                    Else
                        paciente_en_TRA = False
                        mensaje = "Paciente sin intervención Quirúrgica."
                    End If
                    Devuelve.Close()

                End Using

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensaje = ex.Message
            inserta_log("WebService", 0, mensaje)
        End Try

        Return paciente_en_TRA

    End Function

    <WebMethod()> _
    Public Function nuevo_episodio(ByVal datos_usuario As cls_usuario, ByRef datos_paciente As cls_paciente, ByRef datos_informacion As cls_informacion, ByRef mensaje As String) As Boolean

        mensaje = ""

        Try
            Using Conn As New SqlClient.SqlConnection

                Dim str_SQL As String = "INSERT INTO episodio " & _
                                                "(usuario,fecha,doctor,idpaciente,pregunta)" & _
                                                " VALUES " & _
                                                "('" & datos_usuario.usuario & "'," & _
                                                " '" & datos_informacion.fecha & "'," & _
                                                " '" & datos_usuario.nombre & "'," & _
                                                " " & datos_paciente.id_paciente & "," & _
                                                " '" & datos_informacion.pregunta & "')"

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim filas As Integer = Sentencia.ExecuteNonQuery

                If filas = 0 Then
                    nuevo_episodio = False
                Else
                    'Recupera el autonumerico
                    Sentencia.CommandText = "SELECT @@IDENTITY FROM episodio;"
                    datos_informacion.id_episodio = Sentencia.ExecuteScalar()
                    'Traigo todos los datos de informacion y se inicializa la estructura a su nuevo episodio
                    Me.toda_informacion(datos_informacion.id_episodio, datos_paciente, datos_informacion, mensaje)
                    nuevo_episodio = True

                    mensaje = ""
                End If

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensaje = ex.Message
            inserta_log("WebService", 0, "nuevo_episodio:" & mensaje)
        End Try

        Return nuevo_episodio

    End Function

    <WebMethod()> _
    Public Function nuevo_fichero(ByVal tipo As String, _
                                  ByVal extension As String, _
                                  ByVal datos_usuario As cls_usuario, _
                                  ByVal datos_paciente As cls_paciente, _
                                  ByRef datos_informacion As cls_informacion, _
                                  ByRef mensaje As String) As Boolean

        mensaje = ""

        Try
            Using Conn As New SqlClient.SqlConnection
                Dim fecha As String = Format(Now, "yyyyMMdd")

                Dim str_SQL As String = "INSERT INTO fichero " & _
                                                "(id_paciente, idepisodio, fecha, tipofic, nomfic, enlace)" & _
                                                " VALUES " & _
                                                "('" & datos_paciente.id_paciente & "'," & _
                                                " '" & datos_informacion.id_episodio & "'," & _
                                                " '" & Format(Now, "dd/MM/yyyy") & "'," & _
                                                " '" & tipo & "'," & _
                                                " '', " & _
                                                " '')"

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim filas As Integer = Sentencia.ExecuteNonQuery

                If filas = 0 Then
                    nuevo_fichero = False
                Else
                    Dim id As Integer

                    'Recupera el autonumerico
                    Sentencia.CommandText = "SELECT @@IDENTITY FROM fichero;"
                    id = Sentencia.ExecuteScalar()

                    Dim fichero As String = tipo & "_" & fecha & "_" & id & "_" & datos_informacion.id_episodio & extension
                    Dim directorio As String = ""
                    Select Case tipo
                        Case "F"
                            directorio = "foto"
                        Case "V"
                            directorio = "video"
                        Case "A"
                            directorio = "audio"
                    End Select
                    ' Indicar la ubicación del repositorio de ficheros, donde se almacenarán las fotos que se envíen desde la app.
                    Dim enlace As String = "indicad_url_repositorio_ficheros" & directorio & "\" & fichero

                    str_SQL = "UPDATE fichero " & _
                                "SET nomfic = '" & fichero & "'," & _
                                " enlace = '" & enlace & "'" & _
                                " WHERE id = " & id & ";"

                    Sentencia.CommandText = str_SQL
                    Sentencia.Connection = Conn
                    filas = Sentencia.ExecuteNonQuery

                    If filas = 0 Then
                        nuevo_fichero = False
                    Else
                        Select Case tipo
                            Case "F"
                                datos_informacion.fotos = datos_informacion.fotos + 1
                                datos_informacion.veo_enlace = enlace
                                datos_informacion.veo = id
                            Case "V"
                                datos_informacion.videos = datos_informacion.videos + 1
                                datos_informacion.veo_enlace = enlace
                                datos_informacion.veo = id
                            Case "A"
                                datos_informacion.audios = datos_informacion.audios + 1
                                datos_informacion.oigo_enlace = enlace
                                datos_informacion.oigo = id
                        End Select

                        nuevo_fichero = True

                        mensaje = ""
                    End If
                End If

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensaje = ex.Message
            inserta_log("WebService", datos_informacion.id_episodio, "nuevo_fichero:" & mensaje)
        End Try

        Return nuevo_fichero

    End Function

    <WebMethod()> _
    Public Function preguntar_a_gfh(ByVal episodio As Integer, ByVal codser As String, ByRef mensaje As String) As Boolean


        mensaje = ""

        Try
            Using Conn As New SqlClient.SqlConnection

                Dim str_SQL As String = "INSERT INTO respuesta " & _
                                        "(idepisodio, codser, usuario, fecha, doctor, respuesta)" & _
                                        " VALUES " & _
                                        "(" & episodio & "," & _
                                        " '" & codser & "'," & _
                                        " NULL, NULL, NULL, NULL);"

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim filas As Integer = Sentencia.ExecuteNonQuery

                If filas = 0 Then
                    preguntar_a_gfh = False
                Else
                    preguntar_a_gfh = True
                    mensaje = ""
                End If

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensaje = ex.Message
            inserta_log("WebService", 0, mensaje)
        End Try

        Return preguntar_a_gfh

    End Function

    <WebMethod()> _
    Public Function grabar_textos(ByVal datos_usuario As cls_usuario, _
                                  ByVal datos_paciente As cls_paciente, _
                                  ByVal datos_informacion As cls_informacion, _
                                  ByVal str_SQL As String, _
                                  ByRef mensaje As String) As Boolean

        Try

            Using Conn As New SqlClient.SqlConnection

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim filas As Integer = Sentencia.ExecuteNonQuery

                grabar_textos = True
                mensaje = "Información registrada correctamente."
                Conn.Close()

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensaje = ex.Message
            inserta_log("WebService", 0, mensaje)
        End Try


    End Function

    <WebMethod()> _
    Public Function mira_nulo(ByVal dato As Object) As String
        If dato Is DBNull.Value Then
            Return ("")
        Else
            If TypeOf (dato) Is String Then ' solo para string
                dato = Funciones.mira_caracteres(dato) 'miro si la cadena tiene comilla
            End If
            Return CStr(Trim(dato))
        End If

    End Function

    <WebMethod()> _
    Public Function inserta_log(ByVal usuario As String, ByVal id_solicitud As Integer, ByRef texto As String) As Boolean
        Dim str_SQL As String
        'Valida que el codigo corresponda a uno válido
        Using Conn As New SqlClient.SqlConnection

            'Conectamos e insertamos en log
            Try
                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe

                If Conn.State = ConnectionState.Broken Or _
                   Conn.State = ConnectionState.Closed Then
                    Conn.Open()
                End If

                str_SQL = "INSERT INTO log (fecha_hora, usuario, id_solicitud, observaciones) VALUES ('" & Format(Now, "dd/MM/yyyy hh:mm:ss") & "', '" & usuario & "', " & id_solicitud & ", '" & Trim(texto) & "')"

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim filas As Integer = Sentencia.ExecuteNonQuery

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

                Sentencia.Connection.Close()

                If filas = 1 Then
                    Return True
                End If

            Catch ex As Exception
                texto = ex.Message
                Console.WriteLine(ex.Message)
            End Try

            Return False
        End Using


    End Function

    <WebMethod()> _
    Public Function hay_paciente(ByVal nhc As String, ByRef mensaje As String) As Integer

        Using connection As New SqlClient.SqlConnection
            connection.ConnectionString = My.Settings.cadena_conexion_TRAtaMe

            Dim str_SQL As String = "SELECT MAX(id) " & _
                          "FROM paciente " & _
                         "WHERE RTRIM(nhc) = '" & Trim(nhc) & "';"

            Dim command As SqlClient.SqlCommand = New SqlClient.SqlCommand(str_SQL, connection)
            connection.Open()

            Dim reader As SqlClient.SqlDataReader = command.ExecuteReader()

            If reader.HasRows Then
                Do While reader.Read()
                    hay_paciente = reader.GetValue(0)
                Loop
            End If

            reader.Close()

        End Using

    End Function

    <WebMethod()> _
    Public Function existe_paciente(ByVal nhc As String, ByRef mensaje As String) As Integer

        mensaje = ""
        existe_paciente = 0
        Try
            'Valida que exista el máxiom episodio
            Using Conn As New SqlClient.SqlConnection

                mensaje = "Paso:1" & vbCrLf

                Dim str_SQL As String = "SELECT MAX(id) " & _
                                          "FROM paciente " & _
                                         "WHERE RTRIM(nhc) = '" & Trim(nhc) & "';"

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
                mensaje = mensaje & "Paso:2" & vbCrLf

                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                    mensaje = mensaje & "Paso:3" & vbCrLf
                End If

                mensaje = mensaje & "Paso:4" & vbCrLf

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                mensaje = mensaje & "Paso:5" & vbCrLf


                Dim Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader
                mensaje = mensaje & "Paso:6" & vbCrLf

                If Devuelve.HasRows Then
                    mensaje = mensaje & "Paso:7" & vbCrLf

                    Do While Devuelve.Read
                        mensaje = mensaje & "Paso:8" & vbCrLf

                        existe_paciente = Devuelve.GetValue(0)
                    Loop
                End If

                mensaje = mensaje & "Paso:9" & vbCrLf

                Devuelve.Close()

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            inserta_log("WebService", 0, ex.Message)
        End Try

        Return existe_paciente

    End Function

    <WebMethod()> _
    Public Function existe_episodio(ByVal idpaciente As Integer, ByRef mensaje As String) As Integer

        mensaje = ""
        existe_episodio = 0
        Try
            'Valida que exista el máxiom episodio
            Using Conn As New SqlClient.SqlConnection

                Dim str_SQL As String = "SELECT MAX(id) as episodio " & _
                                          "FROM episodio " & _
                                         "WHERE idpaciente = " & idpaciente & ";"


                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader

                If Devuelve.HasRows Then
                    Do While Devuelve.Read
                        existe_episodio = Devuelve.GetValue(0)
                    Loop
                End If
                Devuelve.Close()

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            inserta_log("WebService", 0, ex.Message)
        End Try

        Return existe_episodio

    End Function

    <WebMethod()> _
    Public Function dame_episodios_paciente(ByVal idpaciente As Integer, ByRef mensaje As String, ByRef listax As String, ByRef listan As String) As Boolean

        dame_episodios_paciente = False

        Dim str_SQL As String = "SELECT b.id as episodio ,b.fecha, b.pregunta " & _
                                  "FROM episodio b " & _
                                 "WHERE b.idpaciente = " & idpaciente & _
                                 "ORDER BY b.fecha desc;"

        Using Conn As New SqlClient.SqlConnection

            Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
            If Conn.State = ConnectionState.Closed Or _
               Conn.State = ConnectionState.Broken Then
                Conn.Open()
            End If

            Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)

            Using Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader
                Try
                    listax = ""
                    listan = ""
                    Dim codser As String = ""
                    If Devuelve.HasRows Then
                        Do While Devuelve.Read
                            Dim fecha As Date = Me.mira_nulo(Devuelve.GetValue(1))
                            Dim datoa As String = ""
                            Dim datob As String = ""
                            Dim fch_epi = Format(Devuelve.GetValue(1), "dd/MM/yyyy")

                            'Empleo un corchete como delimitador entre cadenas y para que el Spinner de c# cargue el Array
                            listax = listax & "[" & Devuelve.GetValue(0) & " - " & fch_epi & " - " & Trim(Devuelve.GetValue(2))
                            listan = listan & "[" & Devuelve.GetValue(0)

                        Loop
                        dame_episodios_paciente = True
                    End If

                    Devuelve.Close()


                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    mensaje = ex.Message
                    dame_episodios_paciente = False
                    inserta_log("WebService", 0, ex.Message)
                End Try

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using
        End Using

        Return dame_episodios_paciente

    End Function

    <WebMethod()> _
    Public Function dame_GFH(ByVal servicio As String) As String

        Dim str_SQL As String = "SELECT codser FROM servicio WHERE descripcion = '" & servicio & "';"
        dame_GFH = ""

        Using Conn As New SqlClient.SqlConnection

            Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
            If Conn.State = ConnectionState.Closed Or _
               Conn.State = ConnectionState.Broken Then
                Conn.Open()
            End If

            Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)

            Using Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader
                Try
                    If Devuelve.HasRows Then
                        Do While Devuelve.Read
                            dame_GFH = Me.mira_nulo(Trim(Devuelve.GetValue(0)))
                        Loop
                    End If

                    Devuelve.Close()

                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    inserta_log("WebService", 0, ex.Message)
                    dame_GFH = ""
                End Try

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using
        End Using

        Return dame_GFH

    End Function

    <WebMethod()> _
    Public Function dame_servicios(ByRef mensaje As String, ByRef listax As String, ByRef listan As String) As Boolean

        Dim str_SQL As String = "SELECT descripcion, codser FROM servicio ORDER BY descripcion;"

        Using Conn As New SqlClient.SqlConnection

            Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
            If Conn.State = ConnectionState.Closed Or _
               Conn.State = ConnectionState.Broken Then
                Conn.Open()
            End If

            Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)

            Using Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader
                Try
                    listax = ""
                    listan = ""
                    Dim codser As String = ""
                    If Devuelve.HasRows Then
                        Do While Devuelve.Read

                            'Empleo un corchete como delimitador entre cadenas y para que el Spinner de c# cargue el Array
                            listax = listax & "[" & Me.mira_nulo(Trim(Devuelve.GetValue(0)))
                            listan = listan & "[" & Me.mira_nulo(Trim(Devuelve.GetValue(1)))
                        Loop
                        dame_servicios = True
                    End If

                    Devuelve.Close()

                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    mensaje = ex.Message
                    dame_servicios = False
                    inserta_log("WebService", 0, ex.Message)
                End Try

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using
        End Using

        Return dame_servicios

    End Function

    <WebMethod()> _
    Public Function dame_respuestas_episodio(ByVal idepisodio As Integer, ByRef mensaje As String, ByRef listax As String, ByRef listar As String) As Boolean

        dame_respuestas_episodio = False

        Dim str_SQL As String = "SELECT c.id as respuesta, a.descripcion, b.fecha, c.codser, c.fecha, c.respuesta " & _
                                  "FROM servicio a, episodio b, respuesta c " & _
                                 "WHERE b.id = " & idepisodio & _
                                   "AND c.idepisodio = b.id " & _
                                   "AND c.codser = a.codser " & _
                                   "AND c.respuesta IS NOT NULL " & _
                                 "ORDER BY c.fecha desc;"

        Using Conn As New SqlClient.SqlConnection

            Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
            If Conn.State = ConnectionState.Closed Or _
               Conn.State = ConnectionState.Broken Then
                Conn.Open()
            End If

            Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)

            Using Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader
                Try
                    listax = ""
                    listar = ""
                    Dim lin_id As Integer = 0
                    Dim codser As String = ""
                    If Devuelve.HasRows Then
                        Do While Devuelve.Read
                            Dim fecha As Date = Me.mira_nulo(Devuelve.GetValue(2))
                            Dim datoa As String = ""
                            Dim datob As String = ""
                            Dim infor As String = "RE:NO"
                            Dim fch_resp As String = ""

                            If IsDBNull(Devuelve.GetValue(4)) Then
                                fch_resp = "__/__/____"
                            Else
                                fch_resp = Format(Devuelve.GetValue(4), "dd/MM/yyyy")
                                infor = "RE:SI"
                            End If

                            If lin_id <> Devuelve.GetValue(0) Or codser <> Me.mira_nulo(Trim(Devuelve.GetValue(3))) Then

                                lin_id = Devuelve.GetValue(0)
                                codser = Me.mira_nulo(Trim(Devuelve.GetValue(3)))
                                'Empleo un corchete como delimitador entre cadenas y para que el Spinner de c# cargue el Array
                                listax = listax & "[" & infor & " - " & fch_resp & " - (" & Me.mira_nulo(Trim(Devuelve.GetValue(3))) & ")" & " - " & Me.mira_nulo(Trim(Devuelve.GetValue(1)))
                                listar = listar & "[" & Me.mira_nulo(Trim(Devuelve.GetValue(1))) & " - " & Me.mira_nulo(Trim(Devuelve.GetValue(5)))

                            End If
                        Loop
                        dame_respuestas_episodio = True
                    End If

                    Devuelve.Close()


                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    mensaje = ex.Message
                    dame_respuestas_episodio = False
                    inserta_log("WebService", 0, ex.Message)
                End Try

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using
        End Using

        Return dame_respuestas_episodio

    End Function

    <WebMethod()> _
    Public Function dame_servicios_episodio(ByVal episodio As Integer, ByRef mensaje As String, ByRef listax As String, ByRef listan As String) As Boolean

        dame_servicios_episodio = False

        Dim str_SQL As String = "SELECT a.descripcion, c.fecha " & _
                                  "FROM servicio a, episodio b, respuesta c " & _
                                 "WHERE b.id = " & episodio & _
                                   "AND c.idepisodio = b.id " & _
                                   "AND c.codser = a.codser " & _
                                 "ORDER BY a.descripcion;"

        Using Conn As New SqlClient.SqlConnection

            Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
            If Conn.State = ConnectionState.Closed Or _
               Conn.State = ConnectionState.Broken Then
                Conn.Open()
            End If

            Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)

            Using Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader
                Try
                    listax = ""
                    listan = ""
                    If Devuelve.HasRows Then
                        Do While Devuelve.Read
                            Dim fch_resp As String = ""

                            If IsDBNull(Devuelve.GetValue(1)) Then
                                fch_resp = "__/__/____"
                            Else
                                fch_resp = Format(Devuelve.GetValue(1), "dd/MM/yyyy")
                            End If

                            'Empleo un corchete como delimitador entre cadenas y para que el Spinner de c# cargue el Array
                            listax = listax & "[" & Me.mira_nulo(Trim(Devuelve.GetValue(0)))
                            listan = listan & "[" & fch_resp & " - " & Me.mira_nulo(Trim(Devuelve.GetValue(0)))

                        Loop
                        dame_servicios_episodio = True
                    End If

                    Devuelve.Close()


                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    mensaje = ex.Message
                    dame_servicios_episodio = False
                    inserta_log("WebService", 0, ex.Message)
                End Try

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using
        End Using

        Return dame_servicios_episodio

    End Function

    <WebMethod()> _
    Public Function elimina_pregunta(ByVal episodio As Integer, ByVal codser As String, ByRef mensaje As String) As Boolean

        mensaje = ""

        Try
            Using Conn As New SqlClient.SqlConnection

                Dim str_SQL As String = "DELETE FROM respuesta " & _
                                        " WHERE idepisodio = " & episodio & _
                                        " AND codser = '" & codser & "'" & _
                                        " AND respuesta IS NULL;"

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim filas As Integer = Sentencia.ExecuteNonQuery
                elimina_pregunta = True

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensaje = ex.Message
            elimina_pregunta = False
            inserta_log("WebService", 0, ex.Message)
        End Try

        Return elimina_pregunta

    End Function

    <WebMethod()> _
    Public Function elimina_episodio(ByVal episodio As Integer, ByRef mensaje As String) As Boolean

        mensaje = ""

        Try
            Using Conn As New SqlClient.SqlConnection

                Dim str_SQL As String = "DELETE FROM episodio " & _
                                                " WHERE id = " & episodio & ";"

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim filas As Integer = Sentencia.ExecuteNonQuery

                elimina_episodio = True

                mensaje = ""

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensaje = ex.Message
            inserta_log("WebService", 0, ex.Message)
        End Try

        Return elimina_episodio

    End Function

    <WebMethod()> _
    Public Function elimina_fichero(ByVal tipo As String, ByRef datos_informacion As cls_informacion, ByRef mensaje As String) As Boolean

        mensaje = ""

        Try
            Using Conn As New SqlClient.SqlConnection
                Dim id_fichero As Integer = 0
                Select Case tipo
                    Case "A"
                        id_fichero = datos_informacion.oigo
                    Case Else
                        id_fichero = datos_informacion.veo
                End Select

                Dim str_SQL As String = "DELETE FROM fichero" & _
                                        " WHERE id = " & id_fichero & ";"

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim filas As Integer = Sentencia.ExecuteNonQuery

                If filas = 1 Then

                    Dim pos As Integer = 0
                    Dim fic_ent As String = ""
                    ' Indicad url del host de ftp para el intercambio de los ficheros.
                    Dim host As String = "ftp://ip_servidor_ftp/"

                    Select Case tipo
                        Case "A"
                            pos = InStr(1, Trim(datos_informacion.oigo_enlace), "audio\", CompareMethod.Text)
                            fic_ent = Mid(Trim(datos_informacion.oigo_enlace), pos)
                        Case "F"
                            pos = InStr(1, Trim(datos_informacion.veo_enlace), "foto\", CompareMethod.Text)
                            fic_ent = Mid(Trim(datos_informacion.veo_enlace), pos)
                        Case "V"
                            pos = InStr(1, Trim(datos_informacion.veo_enlace), "video\", CompareMethod.Text)
                            fic_ent = Mid(Trim(datos_informacion.veo_enlace), pos)
                    End Select

                    fic_ent = Replace(fic_ent, "\", "/")

                    Dim request As FtpWebRequest = DirectCast(WebRequest.Create(New Uri(host & fic_ent)), FtpWebRequest)
                    request.Method = WebRequestMethods.Ftp.DeleteFile
                    ' 
                    request.Credentials = New NetworkCredential(UserName, Password)
                    request.UsePassive = False

                    inserta_log("WebService", datos_informacion.id_episodio, "Antes de eliminar, fichero:" & host & fic_ent)

                    Dim response As FtpWebResponse
                    response = CType(request.GetResponse(), FtpWebResponse)
                    response.Close()

                    mensaje = ""
                    elimina_fichero = True
                Else
                    elimina_fichero = False
                    mensaje = "No se ha eliminado el fichero."
                End If

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            mensaje = ex.Message
            inserta_log("WebService", datos_informacion.id_episodio, ex.Message)
        End Try

        Return elimina_fichero

    End Function

    <WebMethod()> _
    Public Function control_respuestas(ByVal episodio As Integer, ByRef lista_si() As String, ByRef lista_no() As String, ByRef lista_ya() As String, ByRef mensaje As String) As Boolean

        control_respuestas = False

        Dim str_SQL As String = "SELECT a.descripcion, c.respuesta " & _
                                  "FROM servicio a, episodio b, respuesta c " & _
                                 "WHERE b.id = " & episodio & _
                                   "AND c.idepisodio = b.id " & _
                                   "AND c.codser = a.codser " & _
                                 "ORDER BY a.descripcion;"

        Using Conn As New SqlClient.SqlConnection

            Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe
            If Conn.State = ConnectionState.Closed Or _
               Conn.State = ConnectionState.Broken Then
                Conn.Open()
            End If

            Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)

            Using Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader
                Try
                    Dim a As Integer = 0
                    Dim b As Integer = 0
                    Dim servicio As String
                    Dim respuesta As String
                    If Devuelve.HasRows Then
                        Do While Devuelve.Read
                            servicio = Trim(Me.mira_nulo(Devuelve.GetValue(0)))
                            respuesta = Trim(Me.mira_nulo(Devuelve.GetValue(1)))
                            For i = 0 To lista_si.Count - 1
                                If servicio = lista_si(i) Then 'Encontrado en la lista_si mandada y en la tabla de respuestas
                                    lista_ya(b) = servicio 'Guardo los que vienen y estan para luego ver los nuevos servicios
                                    b = b + 1
                                    Continue Do
                                End If
                            Next

                            If respuesta = "" Then
                                lista_no(a) = servicio 'Se eliminarán los servicios en lista_no que no tienen respuesta y no estan selecciondos en el movil
                                a = a + 1
                            Else
                                lista_ya(b) = servicio 'Estos servicios se quedaran por tener respuestas
                                b = b + 1
                            End If
                        Loop
                    End If

                    'Comparo las listas, para dejar en lista_si solo los servicios nuevos al episodio que se insertaran al episodio
                    For i = 0 To lista_si.Count - 1
                        If IsDBNull(lista_si(i)) Then Exit For
                        For j = 0 To lista_ya.Count - 1
                            If IsDBNull(lista_si(j)) Then Exit For
                            If lista_si(i) = lista_ya(j) And lista_si(i) <> "" Then
                                lista_si(i) = ""
                                Continue For
                            End If
                        Next
                    Next

                    Devuelve.Close()
                    control_respuestas = True

                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    mensaje = ex.Message
                    control_respuestas = False
                    inserta_log("WebService", 0, "control_respuestas:" & ex.Message)
                End Try

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using
        End Using

        Return control_respuestas

    End Function

    <WebMethod()> _
    Function dame_id_multimed(ByVal tipo As String, ByVal datos_informacion As cls_informacion, ByVal min_max As String) As Integer

        Dim str_SQL As String = "SELECT " & min_max & "(id) " & _
                    " FROM fichero" & _
                    " WHERE idepisodio = " & datos_informacion.id_episodio & _
                    " AND tipofic = '" & tipo & "'"

        dame_id_multimed = 0

        Try
            Using Conn As New SqlClient.SqlConnection
                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe

                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader
                If Devuelve.HasRows Then
                    Do While Devuelve.Read
                        dame_id_multimed = Devuelve.GetValue(0)
                    Loop
                End If
                Devuelve.Close()

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            inserta_log("WebService", datos_informacion.id_episodio, "dame_id_multimed:" & ex.Message)
        End Try

        Return dame_id_multimed

    End Function

    <WebMethod()> _
    Function son_multimedia(ByVal informacion As cls_informacion) As cls_informacion

        Dim str_SQL As String = "SELECT COUNT(*) as son, tipofic" & _
                    " FROM fichero" & _
                    " WHERE idepisodio = " & informacion.id_episodio & _
                    " GROUP BY tipofic"

        son_multimedia = informacion

        son_multimedia.fotos = 0
        son_multimedia.videos = 0
        son_multimedia.audios = 0

        Try
            Using Conn As New SqlClient.SqlConnection
                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe

                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader

                If Devuelve.HasRows Then
                    Do While Devuelve.Read
                        Select Case True
                            Case Devuelve.GetValue(1) = "F"
                                son_multimedia.fotos = Devuelve.GetValue(0)
                            Case Devuelve.GetValue(1) = "V"
                                son_multimedia.videos = Devuelve.GetValue(0)
                            Case Devuelve.GetValue(1) = "A"
                                son_multimedia.audios = Devuelve.GetValue(0)
                        End Select
                    Loop
                End If
                Devuelve.Close()

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            inserta_log("WebService", informacion.id_episodio, "son_multimed:" & ex.Message)
        End Try

        Return son_multimedia

    End Function

    <WebMethod()> _
    Public Function mover_fichero(ByVal fic_entrada() As Byte, _
                                  ByVal tipo As String, _
                                  ByVal extension As String, _
                                  ByVal datos_usuario As cls_usuario, _
                                  ByVal datos_paciente As cls_paciente, _
                                  ByRef datos_informacion As cls_informacion, _
                                  ByRef mensaje As String)
        Try

            If nuevo_fichero(tipo, extension, datos_usuario, datos_paciente, datos_informacion, mensaje) Then
                Dim pos As Integer = 0
                Dim fic_sal As String = ""
                ' Indicad url del host de ftp para el intercambio de los ficheros.
                Dim host As String = "ftp://ip_servidor_ftp/"
                Select Case tipo
                    Case "A"
                        pos = InStr(1, Trim(datos_informacion.oigo_enlace), "audio\", CompareMethod.Text)
                        fic_sal = Mid(Trim(datos_informacion.oigo_enlace), pos)
                    Case "F"
                        pos = InStr(1, Trim(datos_informacion.veo_enlace), "foto\", CompareMethod.Text)
                        fic_sal = Mid(Trim(datos_informacion.veo_enlace), pos)
                    Case "V"
                        pos = InStr(1, Trim(datos_informacion.veo_enlace), "video\", CompareMethod.Text)
                        fic_sal = Mid(Trim(datos_informacion.veo_enlace), pos)
                End Select

                fic_sal = Replace(fic_sal, "\", "/")

                Dim request As FtpWebRequest = WebRequest.Create(New Uri(host & fic_sal))
                request.Method = WebRequestMethods.Ftp.UploadFile
                request.Credentials = New NetworkCredential(UserName, Password)

                inserta_log("WebService", datos_informacion.id_episodio, "Antes de mover, fichero:" & host & fic_sal)

                Dim fileStream() As Byte = fic_entrada
                Dim requestStream As System.IO.Stream = request.GetRequestStream()

                requestStream.Write(fileStream, 0, fileStream.Length)

                requestStream.Close()
                requestStream.Dispose()
                mensaje = "FINAL"
            End If

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensaje = ex.Message
            inserta_log("WebService", datos_informacion.id_episodio, "mover_fichero:" & ex.Message)
        End Try

        Return 0

    End Function

    <WebMethod()> _
    Public Function traer_fichero(ByVal tipo As String, ByVal datos_informacion As cls_informacion) As Byte()
        Try

            Dim pos As Integer = 0
            Dim fic_ent As String = "" 'Formato Request
            Dim fichero As String = "" 'Formato FileStream
            ' Indicad url del host de ftp para el intercambio de los ficheros.
            Dim host As String = "ftp://ip_servidor_ftp/"
            Select Case tipo
                Case "A"
                    pos = InStr(1, Trim(datos_informacion.oigo_enlace), "audio\", CompareMethod.Text)
                    fic_ent = Mid(Trim(datos_informacion.oigo_enlace), pos)
                    fichero = Trim(datos_informacion.oigo_enlace)
                Case "F"
                    pos = InStr(1, Trim(datos_informacion.veo_enlace), "foto\", CompareMethod.Text)
                    fic_ent = Mid(Trim(datos_informacion.veo_enlace), pos)
                    fichero = Trim(datos_informacion.veo_enlace)
                Case "V"
                    pos = InStr(1, Trim(datos_informacion.veo_enlace), "video\", CompareMethod.Text)
                    fic_ent = Mid(Trim(datos_informacion.veo_enlace), pos)
                    fichero = Trim(datos_informacion.veo_enlace)

            End Select

            fic_ent = Replace(fic_ent, "\", "/")
            fichero = Replace(fichero, "\", "/")

            Dim request As FtpWebRequest = WebRequest.Create(New Uri(host & fic_ent))
            request.Method = WebRequestMethods.Ftp.DownloadFile
            request.Credentials = New NetworkCredential(UserName, Password)
            request.UseBinary = True
            inserta_log("WebService", datos_informacion.id_episodio, "Antes de traer, fichero:" & fichero)

            Dim fInfo As New FileInfo(fichero)
            Dim numBytes As Long = fInfo.Length

            Dim fs As New FileStream(fichero, FileMode.Open, FileAccess.Read)
            Dim br As New BinaryReader(fs)
            traer_fichero = br.ReadBytes(CInt(numBytes))
            br.Close()
            fs.Close()


        Catch ex As Exception
            Console.WriteLine(ex.Message)
            inserta_log("WebService", datos_informacion.id_episodio, "traer_fichero:" & ex.Message)
            traer_fichero = New Byte() {}
        End Try

    End Function

End Class

Public Class Funciones
    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    Public Shared Function valida_usuario(ByVal UserName As String, ByVal Password As String) As cls_usuario

        Dim str_SQL As String
        Dim GAI As New ServicioDA
        Dim usuario As New cls_usuario
        Dim WS As New TRAtaMeWS

        'Si esta en el dominio debe encontrarse tambien en la tabla perfiles
        ' Hay que indicar el usuario y el password de conexión al AD.
        If GAI.ValidarUsuario(usuario_conexion_AD, Password_conexion_AD, UserName, Password) = ResultadoLogin.LOGIN_OK Then
            str_SQL = "SELECT per_permiso, per_origen, per_codser, per_codemp, per_dni, per_nombre, per_clave " &
                        "FROM perfiles " &
                       "WHERE per_dni = '" & Trim(UserName) & "';"

            Using Conn As New SqlClient.SqlConnection

                Conn.ConnectionString = My.Settings.cadena_conexion_TRAtaMe

                Try

                    If Conn.State = ConnectionState.Closed Or
                       Conn.State = ConnectionState.Broken Then
                        Conn.Open()
                    End If

                    Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)

                    Dim Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader

                    ' Viene la SELECT per_permiso, per_origen, per_codser, per_codemp, per_dni, per_nombre, per_clave
                    If Devuelve.HasRows Then
                        Devuelve.Read()
                        usuario.permiso = WS.mira_nulo(Devuelve.GetValue(0))
                        usuario.origen = WS.mira_nulo(Devuelve.GetValue(1))
                        usuario.codser = WS.mira_nulo(Devuelve.GetValue(2))
                        usuario.codemp = WS.mira_nulo(Devuelve.GetValue(3))
                        usuario.dni = WS.mira_nulo(Devuelve.GetValue(4))
                        usuario.nombre = WS.mira_nulo(Devuelve.GetValue(5))
                        usuario.usuario = usuario.dni

                        Devuelve.Close()

                    End If

                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                End Try

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        End If

        Return usuario

    End Function

    Public Shared Function mira_caracteres(ByVal dato As String) As String

        Dim a As Integer
        dato = Replace(dato, "'", Chr(96))
        dato = Replace(dato, "<", "")
        dato = Replace(dato, ">", "")
        For a = 128 To 255
            Select Case Chr(a)
                Case "Ñ"
                Case "Á"
                Case "É"
                Case "Í"
                Case "Ó"
                Case "Ú"
                Case "á"
                Case "é"
                Case "í"
                Case "ó"
                Case "ú"
                Case "Ü"
                Case "ü"
                Case Else
                    If Asc(Chr(a)) = 13 Or Asc(Chr(a)) = 10 Then
                    Else
                        dato = Replace(dato, Chr(a), "")
                    End If
            End Select
        Next

        Return CStr(Trim(dato))

    End Function

End Class

<Serializable()> Public Class cls_informacion

    Public id_episodio As Integer = 0
    Public id_paciente As Integer = 0
    Public nhc As String = ""
    Public codser As String = ""
    Public fecha As String = "01/01/1900"
    Public doctor As String = ""
    Public pregunta As String = ""
    Public fch_resp As String = ""
    Public respuesta As String = ""
    Public audios As Integer = 0
    Public fotos As Integer = 0
    Public videos As Integer = 0
    Public veo As Integer = 0
    Public veo_tipo As String = ""
    Public veo_enlace As String = ""
    Public oigo As Integer = 0
    Public oigo_enlace As String = ""

End Class

<Serializable()> Public Class cls_usuario

    Public usuario As String = ""
    Public nombre As String = ""
    Public codser As String = ""
    Public permiso As Integer = 0
    Public origen As String = ""
    Public dni As String = ""
    Public codemp As String = 0
    Public unidad As String = ""
    Public puesto As String = ""
    Public codigo As Integer = 0

End Class

<Serializable()> Public Class cls_paciente

    Public id_paciente As Integer = 0
    Public nhc As String = ""
    Public nombre_com As String = ""
    Public dni As String = ""
    Public cipa As String = ""
    Public telefono As String = ""
    Public fecnac As Date = "01/01/1900"
    Public domicilio As String = ""
    Public numero As String = 0
    Public codpost As String = ""
    Public codpro As String = 0
    Public provincia As String = ""
    Public codmun As String = 0
    Public municipio As String = ""
    Public nombre As String = ""
    Public apellido1 As String = ""
    Public apellido2 As String = ""
    Public numtel2 As String = ""
    Public tipodom As String = ""
    Public calle As String = ""

End Class
