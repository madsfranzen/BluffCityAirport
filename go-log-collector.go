package main

import (
	"encoding/json"
	"fmt"
	"os"
	"strings"

	"github.com/charmbracelet/log"
	amqp "github.com/rabbitmq/amqp091-go"
)

type LogMessage struct {
	Service   string `json:"service"`
	Level     string `json:"level"`
	Message   string `json:"message"`
	Timestamp string `json:"timestamp"`
}

var (
	unknownLog = log.NewWithOptions(os.Stderr, log.Options{
		ReportCaller:    false,
		ReportTimestamp: true,
		Prefix:          "UNKNOWN",
	})

	checkinLog = log.NewWithOptions(os.Stderr, log.Options{
		ReportCaller:    false,
		ReportTimestamp: true,
		Prefix:          "CHECK-IN",
	})

	splitterLog = log.NewWithOptions(os.Stderr, log.Options{
		ReportCaller:    false,
		ReportTimestamp: true,
		Prefix:          "SPLITTER",
	})

	scramblerLog = log.NewWithOptions(os.Stderr, log.Options{
		ReportCaller:    false,
		ReportTimestamp: true,
		Prefix:          "SCRAMBLER",
	})

	resequencerLog = log.NewWithOptions(os.Stderr, log.Options{
		ReportCaller:    false,
		ReportTimestamp: true,
		Prefix:          "RESEQUENCER",
	})
)

func main() {

	fmt.Println("Logger initializing...")

	// Connect to RabbitMQ Server
	conn, err := amqp.Dial("amqp://guest:guest@localhost:5672/")
	failOnError(err, "Failed to connect to RabbitMQ")
	defer conn.Close()

	// Create channel
	ch, err := conn.Channel()
	failOnError(err, "Failed to open a channel")
	defer ch.Close()

	// Declare queue
	q, err := ch.QueueDeclare(
		"logs", // name
		false,  // durable
		false,  // delete when unused
		false,  // exclusive
		false,  // no-wait
		nil,    // arguments
	)
	failOnError(err, "Failed to declare queue")

	// Set up consumer
	msgs, err := ch.Consume(
		q.Name, // queue
		"",     // consumer
		true,   // auto-ack
		false,  // exclusive
		false,  // no-local
		false,  // no-wait
		nil,    // args
	)
	failOnError(err, "Failed to register a Consumer")

	fmt.Println("Logger ready!")

	// Consume forever
	forever := make(chan struct{})

	go func() {
		for m := range msgs {
			var msg map[string]any

			err := json.Unmarshal(m.Body, &msg)
			failOnError(err, "Failed to unmarshal JSON")

			service, _ := msg["service"].(string)
			level, _ := msg["level"].(string)
			message, _ := msg["message"].(string)
			timestamp, _ := msg["timestamp"].(string)

			logHandler(service, level, message, timestamp)
		}
	}()

	<-forever
}

// logHandler logs a message to the appropriate logger based on the service and level.
func logHandler(service string, level string, message string, timestamp string) {

	logger := getLogger(service)

	switch strings.ToLower(level) {
	case "info":
		logger.Info(message, "timestamp", timestamp)
	case "warn", "warning":
		logger.Warn(message, "timestamp", timestamp)
	case "error":
		logger.Error(message, "timestamp", timestamp)
	case "debug":
		logger.Debug(message, "timestamp", timestamp)
	default:
		logger.Fatal(message, "timestamp", timestamp)
	}
}

// getLogger returns the logger instance for a given service.
// If the service is unknown, it returns a default logger with the prefix "UNKNOWN".
func getLogger(service string) *log.Logger {

	switch strings.ToLower(service) {
	case "checkin":
		return checkinLog
	case "splitter":
		return splitterLog
	case "scrambler":
		return scramblerLog
	case "resequencer":
		return resequencerLog
	}
	return unknownLog
}

func failOnError(err error, msg string) {
	if err != nil {
		log.Fatal(msg, "error", err)
	}
}
