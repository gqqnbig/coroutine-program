package main

import "fmt"

// Two groutines are running but deadlock happens

// Output example
//
// Try to receive message
// Try to receive message
// fatal error: all goroutines are asleep - deadlock!

func main() {
	messages := make(chan string)

	go func() {
		fmt.Println("Try to receive message") // Printing
		<-messages                            // Blocking
		fmt.Println("Receive message")        // Never reached
	}()

	fmt.Println("Try to receive message") // Printing
	<-messages                            // Blocking
	fmt.Println("Receive message")        // Never reached

}