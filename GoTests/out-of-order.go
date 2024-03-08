package main

import "fmt"
import "time"

func sum(cInt chan int, cStr chan string) {
	fmt.Println(<-cInt)
	fmt.Println(<-cStr)
}

func main() {
	cInt := make(chan int)
	cStr := make(chan string)
	go sum(cInt, cStr)

	cStr <- "hello"
	cInt <- 1

	time.Sleep(10 * time.Second)
}

