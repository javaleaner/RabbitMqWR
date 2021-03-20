using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqWR
{
    public partial class Form1 : Form
    {
        public ConnectionFactory factory;
        public IConnection connection;
        public IModel channel;
        public Form1()
        {
            InitializeComponent();
            factory = new ConnectionFactory()
            {
                HostName = "localhost",//主机名，Rabbit会拿这个IP生成一个endpoint，这个很熟悉吧，就是socket绑定的那个终结点。
                UserName = "guest",//默认用户名,用户可以在服务端自定义创建，有相关命令行
                Password = "guest",//默认密码
                Port = 5672
            };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            
 
            //using (var connection = factory.CreateConnection())//连接服务器，即正在创建终结点。
            //{
            //    //创建一个通道，这个就是Rabbit自己定义的规则了，如果自己写消息队列，这个就可以开脑洞设计了
            //    //这里Rabbit的玩法就是一个通道channel下包含多个队列Queue
            //    using (var channel = connection.CreateModel())
            //    {
                        channel.QueueDeclare("test1", true, false, false, null);//创建一个名称为kibaqueue的消息队列
                        var properties = channel.CreateBasicProperties();
                        properties.DeliveryMode = 1;
            var prop = properties.Persistent;
                        string message = "I am scott ,now is "+DateTime.Now.ToString("HH:mm:ss"); //传递的消息内容
                        richTextBox1.AppendText(message+"\r\n");
            //          exchange,routingkey,property,msgBody
                        channel.BasicPublish("amq.direct", "test1", properties, Encoding.UTF8.GetBytes(message)); //生产消息
                        //Console.WriteLine($"Send:{message}");
            //    }
            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //var factory = new ConnectionFactory();
            //factory.HostName = "localhost";
            //factory.UserName = "guest";
            //factory.Password = "guest";

            //using (var connection = factory.CreateConnection())
            //{
            //    using (var channel = connection.CreateModel())
            //    {
                    channel.QueueDeclare("test1", true, false, false, null);

                    /* 这里定义了一个消费者，用于消费服务器接受的消息
                     * C#开发需要注意下这里，在一些非面向对象和面向对象比较差的语言中，是非常重视这种设计模式的。
                     * 比如RabbitMQ使用了生产者与消费者模式，然后很多相关的使用文章都在拿这个生产者和消费者来表述。
                     * 但是，在C#里，生产者与消费者对我们而言，根本算不上一种设计模式，他就是一种最基础的代码编写规则。
                     * 所以，大家不要复杂的名词吓到，其实，并没那么复杂。
                     * 这里，其实就是定义一个EventingBasicConsumer类型的对象，然后该对象有个Received事件，
                     * 该事件会在服务接收到数据时触发。
                     */
                    var consumer = new EventingBasicConsumer(channel);//消费者
                    channel.BasicConsume("test1", true, consumer);//消费消息
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        richTextBox2.AppendText(message + "\r\n");
                    };
            //    }
            //}
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            channel.Close();
            connection.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //發送消息2
            string message = textBox1.Text;
            if (message == "")
            {
                MessageBox.Show("消息為空！", "提示", MessageBoxButtons.OK);
                return;
            }
                
            channel.QueueDeclare("test1", true, false, false, null);//创建一个名称为kibaqueue的消息队列
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 1;
            var prop = properties.Persistent;
            message +=  DateTime.Now.ToString("HH:mm:ss"); //传递的消息内容
            richTextBox4.AppendText(message + "\r\n");
            //          exchange,routingkey,property,msgBody
            channel.BasicPublish("helloworld", "test1", null, Encoding.UTF8.GetBytes(message)); //生产消息

		
        }

        private void button3_Click(object sender, EventArgs e)
        {
            channel.QueueDeclare("test1", true, false, false, null);

            /* 这里定义了一个消费者，用于消费服务器接受的消息
             * C#开发需要注意下这里，在一些非面向对象和面向对象比较差的语言中，是非常重视这种设计模式的。
             * 比如RabbitMQ使用了生产者与消费者模式，然后很多相关的使用文章都在拿这个生产者和消费者来表述。
             * 但是，在C#里，生产者与消费者对我们而言，根本算不上一种设计模式，他就是一种最基础的代码编写规则。
             * 所以，大家不要复杂的名词吓到，其实，并没那么复杂。
             * 这里，其实就是定义一个EventingBasicConsumer类型的对象，然后该对象有个Received事件，
             * 该事件会在服务接收到数据时触发。
             */
            var consumer = new EventingBasicConsumer(channel);//消费者
            channel.BasicConsume("test1", true, consumer);//消费消息
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                richTextBox3.AppendText(message + "\r\n");
            };
            channel.BasicAck();
            //consumer.Received += MsgHandle;

        }

        private void MsgHandle(object model, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var msg = Encoding.UTF8.GetString(body);
            richTextBox3.AppendText(msg + "\r\n");
        }
        
    }
}
