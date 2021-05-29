using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqWR
{
    public delegate void Show(string msg,RichTextBox richText);
    public partial class Form1 : Form
    {
        public Show Display;
        public ConnectionFactory factory;
        public IConnection connection;
        public IModel channel;//簡單模式，工作模式
        public IModel subsrcptnModel;//發佈訂閱模式
        public IModel RoutingModel;//路由模式
        public IModel TopicModel;//主題模式
        public Form1()
        {
            InitializeComponent();
            factory = new ConnectionFactory()
            {
                HostName = "localhost",//主机名，Rabbit会拿这个IP生成一个endpoint，这个很熟悉吧，就是socket绑定的那个终结点。
                UserName = "scott",//默认用户名,用户可以在服务端自定义创建，有相关命令行
                Password = "tiger",//默认密码
                Port = 5672
            };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();//簡單模式，工作模式
            subsrcptnModel = connection.CreateModel();//發佈訂閱模式通道
            RoutingModel = connection.CreateModel();//路由模式通道
            TopicModel = connection.CreateModel();//主題模式通道
            CheckForIllegalCrossThreadCalls = false;
            Display = DisplayMsgs;
            
        }
        /// <summary>
        /// 往richBox中寫提示信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="richTextBox"></param>
        public void DisplayMsgs(string msg, RichTextBox richTextBox)
        {
            string message = msg + DateTime.Now.ToString("HHmmssfff"); //传递的消息内容
            richTextBox.AppendText(message + "\r\n");
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
                        string message = "I am scott ,now is "+DateTime.Now.ToString("HHmmssfff"); //传递的消息内容
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
            message =string.Format("{0}<{1}>{2}","生產者2",message,DateTime.Now.ToString("HHmmssfff"))  ; //传递的消息内容
            richTextBox4.AppendText(message + "\r\n");
            //          exchange,routingkey,property,msgBody
            channel.BasicPublish("helloworld", "test1", null, Encoding.UTF8.GetBytes(message)); //生产消息

		
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //接收消息2
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
                Thread.Sleep(1000);
                richTextBox3.AppendText(message +"-消費者2-"+DateTime.Now.ToString("HHmmssfff") +"\r\n");
            };
            //channel.BasicAck();
            //consumer.Received += MsgHandle;

        }
        /// <summary>
        /// 消息事件處理方法consumer.Received += MsgHandle;
        /// </summary>
        /// <param name="model"></param>
        /// <param name="e"></param>
        private void MsgHandle(object model, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var msg = Encoding.UTF8.GetString(body);
            richTextBox3.AppendText(msg + "\r\n");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //消息回執發送3
            string message = textBox2.Text;
            if (message == "")
            {
                MessageBox.Show("消息為空！", "提示", MessageBoxButtons.OK);
                return;
            }

            channel.QueueDeclare("test1", true, false, false, null);//创建一个名称为kibaqueue的消息队列
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;//Non-persistent (1) or persistent (2).設置為2后即使重啟rabbitmq該消息也不會丟失
            var prop = properties.Persistent;
            message = string.Format("{0}<{1}>{2}", "生產者3", message, DateTime.Now.ToString("HHmmssfff")); //传递的消息内容
            richTextBox6.AppendText(message + "\r\n");
            
            //          exchange,routingkey,property,msgBody
            channel.BasicPublish("helloworld", "test1", properties, Encoding.UTF8.GetBytes(message)); //生产消息
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //消息回執接收消息3
            
            channel.QueueDeclare("test1", true, false, false, null);

            
            var consumer = new EventingBasicConsumer(channel);//消费者
            channel.BasicConsume("test1", false, consumer);//消费消息noAck設置為false需要手動確認發送回執
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                Thread.Sleep(6000);
                //channel.BasicAck(ea.DeliveryTag,false);//手動發送確認回執
                channel.BasicNack(ea.DeliveryTag, false, true);//消息處理異常時重新發送
                richTextBox5.AppendText(message + "-消費者3-" + DateTime.Now.ToString("HHmmssfff") + "\r\n");
            };
            
            //consumer.Received += MsgHandle;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //消息回執發送4 訂閱模式發送消息
            //exchange type fanout
            string message = textBox3.Text;
            if (message == "")
            {
                MessageBox.Show("消息為空！", "提示", MessageBoxButtons.OK);
                return;
            }
            subsrcptnModel.ExchangeDeclare("logs","fanout");//定義fanout交換機
            
            message = string.Format("{0}<{1}>{2}", "生產者4", message, DateTime.Now.ToString("HHmmssfff")); //传递的消息内容
            richTextBox8.AppendText(message + "\r\n");
            //第一个参数,向指定的交换机发送消息
            //第二个参数,不指定队列,由消费者向交换机绑定队列
            //如果还没有队列绑定到交换器，消息就会丢失，
            //但这对我们来说没有问题;即使没有消费者接收，我们也可以安全地丢弃这些信息。
            //          exchange,routingkey,property,msgBody
            subsrcptnModel.BasicPublish("logs", "", null, Encoding.UTF8.GetBytes(message)); //生产消息
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //消息接收4
            //自动生成对列名,
            //非持久,独占,自动删除
            string queueName = subsrcptnModel.QueueDeclare().QueueName;
            //把该队列,绑定到 logs 交换机
		    //对于 fanout 类型的交换机, routingKey会被忽略，不允许null值
            subsrcptnModel.QueueBind(queueName,"logs","");
            
            
            var consumer = new EventingBasicConsumer(subsrcptnModel);
            
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body) + "-<消費者4>-"; ;
                //Thread.Sleep(3000);
                //channel.BasicAck(ea.DeliveryTag,false);//手動發送確認回執
                //channel.BasicNack(ea.DeliveryTag, false, true);//消息處理異常時重新發送
                //richTextBox7.AppendText(message + "-消費者4-" + DateTime.Now.ToString("HHmmssfff") + "\r\n");
                Display.BeginInvoke(message, richTextBox7, null, null);
            };
            subsrcptnModel.BasicConsume(queueName, true, consumer);

        }

        private void button9_Click(object sender, EventArgs e)
        {
            //消息接收4-1
            //自动生成对列名,
            //非持久,独占,自动删除
            string queueName = subsrcptnModel.QueueDeclare().QueueName;
            //把该队列,绑定到 logs 交换机
            //对于 fanout 类型的交换机, routingKey会被忽略，不允许null值
            subsrcptnModel.QueueBind(queueName, "logs", "");


            var consumer = new EventingBasicConsumer(subsrcptnModel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body) + "-<消費者4-1>-";
                //Thread.Sleep(3000);
                //channel.BasicAck(ea.DeliveryTag,false);//手動發送確認回執
                //channel.BasicNack(ea.DeliveryTag, false, true);//消息處理異常時重新發送
                //richTextBox9.AppendText(message + "-消費者5-" + DateTime.Now.ToString("HHmmssfff") + "\r\n");
                Display.BeginInvoke(message, richTextBox9, null, null);
            };
            subsrcptnModel.BasicConsume(queueName, true, consumer);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            //路由模式發送消息5
            //消息發送5 路由模式發送消息
            //exchange type fanout
            string message = textBox4.Text;
            if (message == "")
            {
                MessageBox.Show("消息為空！", "提示", MessageBoxButtons.OK);
                return;
            }
            String[] a = { "warning", "info", "error" };
            var rdm = new Random();
            int rdmIdx = rdm.Next()%3;
            string routingKey = a[rdmIdx];

            RoutingModel.ExchangeDeclare("SysLogs", "direct");//定義fanout交換機

            message = string.Format("{0}<{1},{3}>{2}", "生產者5", message, DateTime.Now.ToString("HHmmssfff"),routingKey); //传递的消息内容
            richTextBox12.AppendText(message + "\r\n");
            //第一个参数,向指定的交换机发送消息
            //第二个参数,不指定队列,由消费者向交换机绑定队列
            //如果还没有队列绑定到交换器，消息就会丢失，
            //但这对我们来说没有问题;即使没有消费者接收，我们也可以安全地丢弃这些信息。
            //          exchange,routingkey,property,msgBody
            RoutingModel.BasicPublish("SysLogs", routingKey, null, Encoding.UTF8.GetBytes(message)); //生产消息
        }

        private void button11_Click(object sender, EventArgs e)
        {
            //路由模式 接收消息5
            //自动生成对列名,
            //非持久,独占,自动删除
            string queueName = RoutingModel.QueueDeclare().QueueName;
            //把该队列,绑定到 logs 交换机
            //对于 fanout 类型的交换机, routingKey会被忽略，不允许null值
            RoutingModel.QueueBind(queueName, "SysLogs", "warning");


            var consumer = new EventingBasicConsumer(RoutingModel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body) + "-<消費者5>-"; ;
                //Thread.Sleep(3000);
                //channel.BasicAck(ea.DeliveryTag,false);//手動發送確認回執
                //channel.BasicNack(ea.DeliveryTag, false, true);//消息處理異常時重新發送
                //richTextBox7.AppendText(message + "-消費者4-" + DateTime.Now.ToString("HHmmssfff") + "\r\n");
                Display.BeginInvoke(message, richTextBox11, null, null);
            };
            RoutingModel.BasicConsume(queueName, true, consumer);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //路由模式 接收消息5-1
            
            //自动生成对列名,
            //非持久,独占,自动删除
            string queueName = RoutingModel.QueueDeclare().QueueName;
            //把该队列,绑定到 logs 交换机
            //对于 fanout 类型的交换机, routingKey会被忽略，不允许null值
            RoutingModel.QueueBind(queueName, "SysLogs", "error");


            var consumer = new EventingBasicConsumer(RoutingModel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body) + "-<消費者5-1>-"; ;
                //Thread.Sleep(3000);
                //channel.BasicAck(ea.DeliveryTag,false);//手動發送確認回執
                //channel.BasicNack(ea.DeliveryTag, false, true);//消息處理異常時重新發送
                //richTextBox7.AppendText(message + "-消費者4-" + DateTime.Now.ToString("HHmmssfff") + "\r\n");
                Display.BeginInvoke(message, richTextBox10, null, null);
            };
            RoutingModel.BasicConsume(queueName, true, consumer);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            //主題模式發送消息6
            string message = textBox5.Text;
            if (message == "")
            {
                MessageBox.Show("消息為空！", "提示", MessageBoxButtons.OK);
                return;
            }
            String[] a = { "warning.test.com", "warning.info.test", "error.info.test" };
            var rdm = new Random();
            int rdmIdx = rdm.Next() % 3;
            string routingKey = a[rdmIdx];

            TopicModel.ExchangeDeclare("OracleLogs", "topic");//定義fanout交換機

            message = string.Format("{0}<{1},{3}>{2}", "生產者6", message, DateTime.Now.ToString("HHmmssfff"), routingKey); //传递的消息内容
            richTextBox15.AppendText(message + "\r\n");
            //第一个参数,向指定的交换机发送消息
            //第二个参数,不指定队列,由消费者向交换机绑定队列
            //如果还没有队列绑定到交换器，消息就会丢失，
            //但这对我们来说没有问题;即使没有消费者接收，我们也可以安全地丢弃这些信息。
            //          exchange,routingkey,property,msgBody
            TopicModel.BasicPublish("OracleLogs", routingKey, null, Encoding.UTF8.GetBytes(message)); //生产消息
        }

        private void button14_Click(object sender, EventArgs e)
        {
            //主題模式接收消息6
            //路由模式 接收消息6
            //自动生成对列名,
            //非持久,独占,自动删除
            string queueName = TopicModel.QueueDeclare().QueueName;
            //把该队列,绑定到 logs 交换机
            //对于 fanout 类型的交换机, routingKey会被忽略，不允许null值
            TopicModel.QueueBind(queueName, "OracleLogs", "warning.*.*");
            

            var consumer = new EventingBasicConsumer(RoutingModel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body) + "-<消費者6>-"; ;
                //Thread.Sleep(3000);
                //channel.BasicAck(ea.DeliveryTag,false);//手動發送確認回執
                //channel.BasicNack(ea.DeliveryTag, false, true);//消息處理異常時重新發送
                //richTextBox7.AppendText(message + "-消費者4-" + DateTime.Now.ToString("HHmmssfff") + "\r\n");
                Display.BeginInvoke(message, richTextBox14, null, null);
            };
            TopicModel.BasicConsume(queueName, true, consumer);

        }

        private void button13_Click(object sender, EventArgs e)
        {
            //主題模式接收消息6-1
            //路由模式 接收消息6
            //自动生成对列名,
            //非持久,独占,自动删除
            string queueName = TopicModel.QueueDeclare().QueueName;
            //把该队列,绑定到 logs 交换机
            //对于 fanout 类型的交换机, routingKey会被忽略，不允许null值
            TopicModel.QueueBind(queueName, "OracleLogs", "*.*.test");
            

            var consumer = new EventingBasicConsumer(RoutingModel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body) + "-<消費者6-1>-"; ;
                //Thread.Sleep(3000);
                //channel.BasicAck(ea.DeliveryTag,false);//手動發送確認回執
                //channel.BasicNack(ea.DeliveryTag, false, true);//消息處理異常時重新發送
                //richTextBox7.AppendText(message + "-消費者4-" + DateTime.Now.ToString("HHmmssfff") + "\r\n");
                Display.BeginInvoke(message, richTextBox13, null, null);
            };
            TopicModel.BasicConsume(queueName, true, consumer);
        }
        
    }
}
