package main

import "fmt"
import "time"

func main() {
	messages := make(chan string)

	// In spawned goroutine
	//
	// Send message to channel
	// But before do it sleep for a while
	go func() {
		time.Sleep(1000 * time.Millisecond)
		messages <- "Hello"
	}()

	// In main goroutine
	//
	// Receive message from channel
	// Message appears after spawned goroutine awaked from sleeping
	// Channel is empty until other goroutine send message
	// So this receiving will be blocked for a while
	fmt.Println(<-messages) // Hello
}