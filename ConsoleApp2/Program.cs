// See https://aka.ms/new-console-template for more information
using IBM.XMS;

XMSFactoryFactory factoryFactory;
IConnectionFactory cf;
IDestination destination;
IMessageConsumer consumerAsync;
MessageListener messageListener;
// Get an instance of factory.
factoryFactory = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);

// Create WMQ Connection Factory.
cf = factoryFactory.CreateConnectionFactory();

// Set the properties
cf.SetStringProperty(XMSC.WMQ_HOST_NAME, "5.tcp.eu.ngrok.io");
cf.SetIntProperty(XMSC.WMQ_PORT, 16757);
cf.SetStringProperty(XMSC.WMQ_CHANNEL, "DEV.ADMIN.SVRCONN");
cf.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
cf.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, "QM1");
cf.SetStringProperty(XMSC.USERID, "admin");
cf.SetStringProperty(XMSC.PASSWORD, "passw0rd");

// Create connection.
var connectionWMQ = cf.CreateConnection();
// Create session with client acknowledge so that we can acknowledge 
// only if message is sent to Azure Service Bus queue
var sessionWMQ = connectionWMQ.CreateSession(false, AcknowledgeMode.ClientAcknowledge);
// Create destination
destination = sessionWMQ.CreateQueue("DEV.QUEUE.1");
// Create consumer
consumerAsync = sessionWMQ.CreateConsumer(destination);

// Setup a message listener and assign it to consumer
messageListener = new MessageListener(OnMessageCallback);
consumerAsync.MessageListener = messageListener;

// Start the connection to receive messages.
connectionWMQ.Start();

// Wait for messages till a key is pressed by user
Console.ReadKey();

// Cleanup
consumerAsync.Close();
destination.Dispose();
sessionWMQ.Dispose();
connectionWMQ.Close();

void OnMessageCallback(IMessage message)
{
    try
    {
        Console.WriteLine($"{message.ToString()}");

        message.Acknowledge();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Exception caught in OnMessageCallback: {0}", ex);
    }
} // end OnMessageCallback