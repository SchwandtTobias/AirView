//
//  ViewController.m
//  AirView
//
//  Created by Tobias Schwandt on 01.03.12.
//  Copyright (c) 2012 Zebresel. All rights reserved.
//

#import "ViewController.h"

@implementation ViewController
@synthesize tbUrl;

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Release any cached data, images, etc that aren't in use.
}

- (BOOL)textFieldShouldReturn:(UITextField *)textField
{
    [textField resignFirstResponder];
    return YES;
}

#pragma mark - View lifecycle

- (void)viewDidLoad
{
    [super viewDidLoad];
	// Do any additional setup after loading the view, typically from a nib.
}

- (void)viewDidUnload
{
    [self setTbUrl:nil];
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (void)viewWillAppear:(BOOL)animated
{
    [super viewWillAppear:animated];
}

- (void)viewDidAppear:(BOOL)animated
{
    [super viewDidAppear:animated];
}

- (void)viewWillDisappear:(BOOL)animated
{
	[super viewWillDisappear:animated];
}

- (void)viewDidDisappear:(BOOL)animated
{
	[super viewDidDisappear:animated];
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation
{
    // Return YES for supported orientations
    return (interfaceOrientation != UIInterfaceOrientationPortraitUpsideDown);
}

- (IBAction)flyTo:(id)sender {
    NSString *address = [[tbUrl text] stringByAppendingString:@"addData.php?"];
    
    switch ([sender tag]) {
        case 0:
            //Fly Forward
            address = [address stringByAppendingString:@"nick=50&roll=125"];
            break;
        case 1:
            //Fly Backward
            address = [address stringByAppendingString:@"nick=200&roll=125"];
            break;
        case 2:
            //Fly Left
            address = [address stringByAppendingString:@"nick=125&roll=50"];
            break;
        case 3:
            //Fly Right
            address = [address stringByAppendingString:@"nick=125&roll=200"];
            break;
            
        default:
            address = [address stringByAppendingString:@"nick=125&roll=125"];
            break;
    }
    
    NSURL *url = [NSURL URLWithString:address];
    NSURLRequest *request = [NSURLRequest requestWithURL:url];
    
    NSURLConnection *connection = [NSURLConnection connectionWithRequest:request delegate:self];
}
@end
