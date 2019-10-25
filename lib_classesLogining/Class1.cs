using System;
using System.Data;
using System.Data.SqlClient;

public delegate void Metod_Log(clsLog log);
public class clsLog
/* Структура лога события 
 */
{
    public DateTime time;
    public int danger;
    public string name; // имя инициатора сообщения
    public string comment;
    public int id_foms_region;
    public int id_task;
    public DateTime date_start;
    public DateTime date_finish;
    
    public clsLog(DateTime time_, int danger_, string name_, int id_foms_region_, int id_task_, DateTime date_start_, DateTime date_finish_, string comment_)
    {
        time = time_;
        danger = danger_;
        name = name_;
        id_foms_region = id_foms_region_;
        date_start = date_start_;
        date_finish = date_finish_;
        id_task = id_task_;
        comment = comment_;
    }
}

public class clsLogining
/* Определяет процедуру записи логов в базу
 * 
 * 
 */
{
    public string server_connection_string = "";
    public string queryUpdate;
    public SqlConnection server_connection;
    public SqlCommand server_command;
    public Metod_Log metod_log;
    public string adminMail;
    private string MailServerIp;
    private string gatewayMail;
    private string gatewayPassword;
    public bool adminInform = false;
    public clsLogining(Metod_Log metod_log_, string connection, bool adminInform_ = false, string adminMail_= null/*"ex.foms@gmail.com"*/, string MailServerIp_ = null/*"192.168.1.2"*/, string gatewayMail_ = null/*"AOFOMS Gateware server-shrk@aofoms.tsl.ru"*/, string gatewayPassword_ = null)
    {
        metod_log = metod_log_;
        adminInform = adminInform_;
        adminMail = adminMail_;
        MailServerIp = MailServerIp_;
        gatewayMail = gatewayMail_;
        gatewayPassword = gatewayPassword_;

        server_connection_string = connection;
        server_connection = new SqlConnection(server_connection_string);
        server_command = new SqlCommand("insert into log (DANGER, DATE, NAME, ID_FOMS_REGION, ID_TASK, DATE_START, DATE_FINISH, COMMENT) values (@DANGER, @DATE, @NAME, @ID_FOMS_REGION, @ID_TASK, @DATE_START, @DATE_FINISH, @COMMENT)");
        server_command.Connection = server_connection;
        server_command.Parameters.Add("@DANGER", SqlDbType.Int);
        server_command.Parameters.Add("@DATE", SqlDbType.DateTime);
        server_command.Parameters.Add("@NAME", SqlDbType.Text);
        server_command.Parameters.Add("@ID_FOMS_REGION", SqlDbType.Int);
        server_command.Parameters.Add("@ID_TASK", SqlDbType.Int);
        server_command.Parameters.Add("@DATE_START", SqlDbType.DateTime);
        server_command.Parameters.Add("@DATE_FINISH", SqlDbType.DateTime);
        server_command.Parameters.Add("@COMMENT", SqlDbType.Text);
    }
    public bool Server_connecting()
    {
        bool status;
        try
        {
            server_command.Connection.Open();
            status = true;
        }
        catch(Exception e)
        {
            metod_log(new clsLog(DateTime.Now, 1, "shedule", 0, 0, DateTime.Now, DateTime.Now, "запись в лог - " + e.Message));
            status = false;
        }
        return status;
    }
    public void Server_Close()
    {
        server_connection.Close();
    }
    public bool Server_insert(clsLog log)
    {
        server_command.Parameters["@DANGER"].Value = log.danger;
        server_command.Parameters["@DATE"].Value = log.time;
        server_command.Parameters["@NAME"].Value = log.name;
        server_command.Parameters["@ID_FOMS_REGION"].Value = log.id_foms_region;
        server_command.Parameters["@ID_TASK"].Value = log.id_task;
        server_command.Parameters["@DATE_START"].Value = log.date_start;
        server_command.Parameters["@DATE_FINISH"].Value = log.date_finish;
        server_command.Parameters["@COMMENT"].Value = log.comment;

        if (log.danger < 1 && adminInform)
        {
            clsLibrary.SendMail(
                            MailServerIp, //"192.168.1.2",
                            gatewayMail, //"AOFOMS Gateware server-shrk@aofoms.tsl.ru",
                            gatewayPassword,// "A3z4y5",
                            adminMail, //"sa.gate.aofoms@yandex.ru",
                            "Внимание!",
                            log.name + "|" + log.comment);
        }
        bool status;
        try
        {
            server_command.ExecuteNonQuery();
            status = true;
        }
        catch
        {
            status = false;
        }
        return status;
    }
    public bool Server_status()
    {
        return (true);
    }
}