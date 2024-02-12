package main

import "fmt"
import "time"

func main() {
	messages := make(chan string)

	// Receiver
	go func() {
		fmt.Println("Receiver : I am waiting for your message.")
		msg := <-messages
		fmt.Println("Receiver : I got a mail.")
		fmt.Println(msg)
	}()

	// Sender
	time.Sleep(2000 * time.Millisecond)
	messages <- "Message : Do you like go langage?"

	// Wait spawned goroutine process
	time.Sleep(1000 * time.Millisecond)
}