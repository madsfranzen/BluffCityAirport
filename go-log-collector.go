package main

import (
	"os"

	"github.com/charmbracelet/log"
	// amqp "github.com/rabbitmq/amqp091-go"
)

func main() {

	checkinLog := log.NewWithOptions(os.Stderr, log.Options{
		ReportCaller:    false,
		ReportTimestamp: true,
		Prefix:          "Check In",
	})

	checkinLog.Info("Hello World!")
}
