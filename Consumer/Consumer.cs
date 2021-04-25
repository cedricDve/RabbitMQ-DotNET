using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Consumer
{
    //-- HANDLEIDING --- APP RUNNEN---------------------------------------------------------------------------------------
    // Best EERST DE CONSUMER RUNNEN zodat deze de tijd heeft om te "subscriben".
    // Vervolgens kan je de Producer RUNNEN ( Je hebt hiervoor 2 VisualStudio Vensters voor nodig)
    // Als alles goed gaat wordt dan een bericht vanuit de Producer (via RabbitMQ) op een Queue geplaatst,
    // dat vervolgens door de Consumer wordt afgehaald(uitgelezen).

    /* 
    ----DOEL----------------------------------------------------------------------------
     In deze App is het de bedoeling om een Consumer "na te bouwen".
     Het doel is om message van de queue("demo-queue) te halen (RabbitMQ).

    ----DOCKER -------------------------------------------------------------------------
     We gebruiken hiervoor Docker, het volgende commendo zorgt ervoor dat je een rabbitMQ instantie kan aanmaken 
     met het management platform. (Om zo te testen of wat je doet wel werkt)

      =>  docker run -d --hostname localhost --name rabbitMQ -p 15672:15672 -p 5672:5672 rabbitmq:3.8-management 

        * Als je dit commando gebruikt zal RabbitMQ standaart een User:"guest" met Wachtwoord:"guest" aanmaken.
        * Dit kan je teste met je browser: localhost:15672
     */
    static class Consumer
    {
        // Om een connectie te maken met RabbitMQ hebben we nood aan een package dat we kunnen toevoegen mbv. NuGet.
        // => Package "RabbitMQ.Client"


        static void Main(string[] args)
        {
            //---CONNECTIE MAKEN MET RABBITMQ-------------------------------------------------------------------------
            // Om een connectie te kunnen maken met RabbitMQ gebruiken we een "ConnectionFactory".
            // We maken een niewe ConnectionFactory, hier geven we een URI aan mee.
            // ! Hiervoor moet je de RabbitMQ.Client namespace toevoegen:  " using RabbitMQ.Client"

            var factory = new ConnectionFactory
            {
                // De Uri volgt de AMQP pattern,
                // je geeft eerst de gebruikersnaam gevolgd door ":" met het wachtwoord gevolgd door "@" met naam van de host gevolgd door ":"  de poortnummer
                Uri = new Uri("amqp://guest:guest@localhost:5672")
            };
            // Nu kunnen we de ConnectionFactory gebruiken om een connectie aan te maken.
            // We gebruiken hiervoor de default "CreateConnection"  => retruns an IConnection Object!
            using var connection = factory.CreateConnection();

            //---CHANNEL AANMAKEN--------------------------------------------------------------------------------------
            // Nu we een verbinding hebben met RabbitMQ, moeten we nog een Channel aanmaken.
            // We gebruiken hiervoor de default"CreateModel" => returns an IModel (returns the channel created)
            using var channel = connection.CreateModel();

            //---QUEUE DECLAREREN---------------------------------------------------------------------------------------
            // Nu we een channel hebben kunnen we onze Queue declareren.
            // We gebruiken hiervoor onze aangemaakte channel(IModel) en roepen "QueueDeclare" hierop.
            // * We kunnen een Queue-naam meegeven,
            // * durable op "true" zetten(zodat de message op de Queue blijft zolang deze niet door een Consumer wordt afgehaald),
            // * de andere properties zet je best nu op "false", en agruments geef je niets mee(null).(Deze zijn enkel belangrijk voor de Exchanges!)

            channel.QueueDeclare("demo-queue", durable: true, exclusive: false, autoDelete: false, arguments: null);


            //---CONSUMER AANMAKEN----------------------------------------------------------------------------
            // We gebruiken hiervoor "EventingBasicConsumer" deze verwacht een IModel
            var consumer = new EventingBasicConsumer(channel);

            //---BERICHT AFHALEN VAN DE QUEUE----------------------------------------------------------------------------
            consumer.Received += (sender, e) =>
            {
                // we willen nu de Byte-Array van de message ophalen
                var body = e.Body.ToArray();
                // deze Byte-Array zetten we dan om naar een string
                var message = Encoding.UTF8.GetString(body);

                //We schrijven dit een in de console
                Console.WriteLine(message);
            };

            //---CONSUMER VERBINDEN MET DE QUEUE-- BERICHTEN AFHALEN----------------------------------------------------------------------------
            // We gebruiken BasicConsume, deze verwacht:
            // * naam van de queue, we zetten autoAck op true, en geven een IBasicConsumer mee
            channel.BasicConsume("demo-queue", true, consumer);

            Console.ReadLine();

        }
    }
}


