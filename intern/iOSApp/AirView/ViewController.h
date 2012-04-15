//
//  ViewController.h
//  AirView
//
//  Created by Tobias Schwandt on 01.03.12.
//  Copyright (c) 2012 Zebresel. All rights reserved.
//

#import <UIKit/UIKit.h>

@interface ViewController : UIViewController<UITextFieldDelegate>

- (IBAction)flyTo:(id)sender;
@property (weak, nonatomic) IBOutlet UITextField *tbUrl;
@end
